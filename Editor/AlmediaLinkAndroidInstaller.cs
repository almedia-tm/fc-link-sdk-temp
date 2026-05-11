using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace AlmediaLink.Editor
{
    /// <summary>
    /// Reads the package's <c>AlmediaLinkDependencies.xml</c> (the EDM4U-format source of
    /// truth) and merges equivalent gradle <c>implementation</c> lines into the host's
    /// <c>Assets/Plugins/Android/mainTemplate.gradle</c>. Idempotent — safe to re-run after
    /// SDK updates; replaces an existing marker block in place.
    ///
    /// Hosts who already have EDM4U installed don't need this — EDM4U auto-resolves the
    /// XML on save / before build. This menu is the manual fallback for hosts without EDM4U.
    /// </summary>
    internal static class AlmediaLinkAndroidInstaller
    {
        private const string PkgDepsFile =
            "Packages/com.almedia.link/Plugins/Android/AlmediaLinkDependencies.xml";

        private const string TargetPath = "Assets/Plugins/Android/mainTemplate.gradle";

        // Marker comments delimit the block that this installer manages. Edits inside
        // the markers are overwritten on next install.
        private const string StartMarker = "// >>> almedia-link deps (managed by Almedia → Install Android Dependencies)";
        private const string EndMarker = "// <<< almedia-link deps";

        [MenuItem("Almedia/Install Android Dependencies")]
        public static void Install()
        {
            if (!File.Exists(PkgDepsFile))
            {
                Fail($"Package dependency file missing:\n{PkgDepsFile}\n\n" +
                     "The Almedia Link package may be corrupted — re-import or reinstall.");
                return;
            }

            var depLines = ExtractDependencyLines(File.ReadAllText(PkgDepsFile));
            if (depLines.Count == 0)
            {
                Fail($"No <androidPackage> entries found in:\n{PkgDepsFile}");
                return;
            }

            if (!File.Exists(TargetPath))
            {
                if (!TryCopyUnityDefaultTemplate(TargetPath, out string copyError))
                {
                    Fail($"{TargetPath} does not exist and Unity's bundled template could not be copied.\n\n" +
                         $"{copyError}\n\n" +
                         "Enable 'Custom Main Gradle Template' in Player Settings → Android → Publishing Settings, then re-run.");
                    return;
                }
            }

            string template = File.ReadAllText(TargetPath);
            string block = BuildMarkerBlock(depLines);

            string updated = ReplaceOrInsertBlock(template, block, out string mergeError);
            if (updated == null)
            {
                Fail($"Could not merge dependencies into {TargetPath}.\n\n{mergeError}");
                return;
            }

            if (updated == template)
            {
                Info($"Android dependencies already up to date at {TargetPath}.");
                return;
            }

            try
            {
                File.WriteAllText(TargetPath, updated);
            }
            catch (Exception ex)
            {
                Fail($"Failed to write {TargetPath}:\n{ex.Message}");
                return;
            }

            AssetDatabase.Refresh();
            Debug.Log($"[AlmediaLink] Android dependencies installed at {TargetPath} ({depLines.Count} entries).");
            Info($"Installed {depLines.Count} dependencies into:\n{TargetPath}\n\n" +
                 "Block is bracketed by '// >>> almedia-link deps' / '// <<< almedia-link deps'. " +
                 "Re-run this command after SDK updates to pull new versions.");
        }

        // ---- helpers ----

        /// <summary>
        /// Parses the EDM4U-format XML and returns one gradle <c>implementation '...'</c>
        /// line per <c>&lt;androidPackage spec="..."/&gt;</c> element.
        /// </summary>
        private static List<string> ExtractDependencyLines(string xml)
        {
            var lines = new List<string>();

            var doc = new XmlDocument();
            try
            {
                doc.LoadXml(xml);
            }
            catch (XmlException ex)
            {
                Debug.LogError($"[AlmediaLink] Failed to parse {PkgDepsFile}: {ex.Message}");
                return lines;
            }

            var nodes = doc.SelectNodes("//androidPackage");
            if (nodes == null) return lines;

            foreach (XmlNode node in nodes)
            {
                var spec = node.Attributes?["spec"]?.Value;
                if (string.IsNullOrWhiteSpace(spec)) continue;
                lines.Add($"implementation '{spec.Trim()}'");
            }
            return lines;
        }

        private static int FindMatchingBrace(string text, int openIdx)
        {
            if (openIdx < 0 || openIdx >= text.Length) return -1;
            int depth = 0;
            for (int i = openIdx; i < text.Length; i++)
            {
                if (text[i] == '{') depth++;
                else if (text[i] == '}')
                {
                    depth--;
                    if (depth == 0) return i;
                }
            }
            return -1;
        }

        private static string BuildMarkerBlock(List<string> depLines)
        {
            var sb = new StringBuilder();
            sb.AppendLine("    " + StartMarker);
            foreach (var line in depLines)
                sb.AppendLine("    " + line);
            sb.Append("    " + EndMarker);
            return sb.ToString();
        }

        private static string ReplaceOrInsertBlock(string template, string newBlock, out string error)
        {
            error = null;

            int startIdx = template.IndexOf(StartMarker, StringComparison.Ordinal);
            int endIdx = template.IndexOf(EndMarker, StringComparison.Ordinal);

            if (startIdx >= 0 && endIdx >= 0 && endIdx > startIdx)
            {
                int lineStart = template.LastIndexOf('\n', startIdx) + 1;
                int lineEnd = template.IndexOf('\n', endIdx);
                if (lineEnd < 0) lineEnd = template.Length;
                return template.Substring(0, lineStart) + newBlock + template.Substring(lineEnd);
            }

            if (startIdx >= 0 || endIdx >= 0)
            {
                error = "Found a partial Almedia marker block (only the start or end marker present). " +
                        "Open mainTemplate.gradle and remove the stray marker, then re-run.";
                return null;
            }

            int depsKw = template.IndexOf("dependencies", StringComparison.Ordinal);
            if (depsKw < 0)
            {
                error = "No 'dependencies' block found. The template may be malformed.";
                return null;
            }
            int braceOpen = template.IndexOf('{', depsKw);
            int braceClose = FindMatchingBrace(template, braceOpen);
            if (braceOpen < 0 || braceClose < 0)
            {
                error = "Could not parse the 'dependencies { ... }' block.";
                return null;
            }

            int insertAt = template.LastIndexOf('\n', braceClose) + 1;
            return template.Substring(0, insertAt) + newBlock + "\n" + template.Substring(insertAt);
        }

        private static bool TryCopyUnityDefaultTemplate(string dstPath, out string error)
        {
            string src = Path.Combine(
                EditorApplication.applicationContentsPath,
                "PlaybackEngines/AndroidPlayer/Tools/GradleTemplates/mainTemplate.gradle");

            if (!File.Exists(src))
            {
                error = $"Unity's bundled template not found at:\n{src}";
                return false;
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dstPath));
                File.Copy(src, dstPath);
                AssetDatabase.Refresh();
                Debug.Log($"[AlmediaLink] Created {dstPath} from Unity's bundled template.");
                error = null;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static void Fail(string message)
        {
            Debug.LogError("[AlmediaLink] " + message);
            EditorUtility.DisplayDialog("Almedia Link — Install Android Dependencies", message, "OK");
        }

        private static void Info(string message)
        {
            EditorUtility.DisplayDialog("Almedia Link — Install Android Dependencies", message, "OK");
        }
    }
}
