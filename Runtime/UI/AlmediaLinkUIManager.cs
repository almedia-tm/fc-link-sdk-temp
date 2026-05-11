using System.Collections.Generic;
using AlmediaLink.Models;
using UnityEngine;
using UnityEngine.UI;

namespace AlmediaLink.UI
{
    internal static class AlmediaLinkUIManager
    {
        private const int MaxDisplayedNotifications = 3;

        private static NotificationCardController _notificationCard;
        private static ActivityOverlayController _activityOverlay;
        private static List<AlmediaNotification> _lastNotifications;
        private static bool _initialized;
        private static readonly HashSet<string> _missingPrefabsLogged = new HashSet<string>();

        internal static void Initialize()
        {
            if (_initialized) return;
            AlmediaLinkSDK.OnNotifications += HandleNotifications;
            NotificationCardController.OnStackedCardTapped += HandleStackedCardTapped;
            _initialized = true;
        }
        
        internal static void ShowATTPrePrompt()
        {
            var prefab = LoadPrefabOrError<ATTPrePromptController>(
                "Prefabs/ATTPrePrompt", "ATT pre-prompt",
                AlmediaLinkSettings.Load()?.AttPrePromptOverride);
            if (prefab == null)
            {
                AlmediaLinkSDK.ContinueWithATT();
                return;
            }

            var instance = Object.Instantiate(prefab);
            instance.Show();
        }

        internal static void ShowLinkPopup()
        {
            var prefab = LoadPrefabOrError<LinkPopupController>(
                "Prefabs/LinkPopup", "Link popup",
                AlmediaLinkSettings.Load()?.LinkPopupOverride);
            if (prefab == null) return;

            var instance = Object.Instantiate(prefab);
            instance.Show();
        }

        internal static void Cleanup()
        {
            AlmediaLinkSDK.OnNotifications -= HandleNotifications;
            NotificationCardController.OnStackedCardTapped -= HandleStackedCardTapped;
            _initialized = false;
            _lastNotifications = null;
            _missingPrefabsLogged.Clear();

            if (_notificationCard != null)
            {
                var canvas = _notificationCard.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
#if UNITY_EDITOR
                    Object.DestroyImmediate(canvas.gameObject);
#else
                    Object.Destroy(canvas.gameObject);
#endif
                }
            }
            _notificationCard = null;
            _activityOverlay = null; // destroyed with canvas (child)
        }

        private static void HandleNotifications(List<AlmediaNotification> notifications)
        {
            if (!ShouldShowDefaultNotificationUI()) return;
            _lastNotifications = CapToLatest(notifications);
            EnsureNotificationCardExists();
            if (_notificationCard != null)
                _notificationCard.ShowNotifications(_lastNotifications);
        }

        private static List<AlmediaNotification> CapToLatest(List<AlmediaNotification> notifications)
        {
            if (notifications == null || notifications.Count <= MaxDisplayedNotifications)
                return notifications;
            return notifications.GetRange(notifications.Count - MaxDisplayedNotifications, MaxDisplayedNotifications);
        }

        private static void HandleStackedCardTapped()
        {
            if (_lastNotifications == null || _lastNotifications.Count == 0) return;
            EnsureActivityOverlayExists();
            if (_activityOverlay != null)
                _activityOverlay.Show(_lastNotifications);
        }

        private static bool ShouldShowDefaultNotificationUI()
        {
            var settings = AlmediaLinkSettings.Load();
            return settings != null && settings.EnableDefaultNotificationUI;
        }

        private static void EnsureNotificationCardExists()
        {
            if (_notificationCard != null) return;

            var prefab = LoadPrefabOrError<NotificationCardController>(
                "Prefabs/NotificationCard", "Default notification UI",
                AlmediaLinkSettings.Load()?.NotificationCardOverride);
            if (prefab == null) return;

            var canvasGo = new GameObject("AlmediaLinkNotificationCanvas");
            Object.DontDestroyOnLoad(canvasGo);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(780, 1688);
            scaler.matchWidthOrHeight = 0.8f;

            canvasGo.AddComponent<GraphicRaycaster>();

            _notificationCard = Object.Instantiate(prefab, canvasGo.transform);
        }

        private static void EnsureActivityOverlayExists()
        {
            if (_activityOverlay != null) return;

            var prefab = LoadPrefabOrError<ActivityOverlayController>(
                "Prefabs/ActivityOverlay", "Activity overlay",
                AlmediaLinkSettings.Load()?.ActivityOverlayOverride);
            if (prefab == null) return;

            // Reuse the existing notification canvas
            if (_notificationCard == null)
            {
                AlmediaLog.Warning("Cannot create ActivityOverlay: NotificationCard does not exist.");
                return;
            }
            var canvas = _notificationCard.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                AlmediaLog.Warning("Cannot create ActivityOverlay: Notification Canvas not found.");
                return;
            }

            _activityOverlay = Object.Instantiate(prefab, canvas.transform);
        }

        /// <summary>
        /// Resolves a prefab to instantiate. If <paramref name="overrideRef"/> is non-null,
        /// it wins (host-provided Prefab Variant). Otherwise loads the SDK default from
        /// <c>Resources/{path}</c>, logging a one-shot error if the default is missing.
        /// </summary>
        private static T LoadPrefabOrError<T>(string path, string featureName, T overrideRef = null) where T : Object
        {
            if (overrideRef != null) return overrideRef;

            var prefab = Resources.Load<T>(path);
            if (prefab == null && _missingPrefabsLogged.Add(path))
            {
                AlmediaLog.Error(
                    $"{typeof(T).Name} prefab missing at 'Resources/{path}'. {featureName} is disabled. " +
                    "Verify the AlmediaLink package was imported correctly, or disable the default UI in AlmediaLinkSettings.");
            }
            return prefab;
        }
    }
}
