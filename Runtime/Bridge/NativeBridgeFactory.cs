using UnityEngine;

namespace AlmediaLink.Bridge
{
    internal static class NativeBridgeFactory
    {
        internal const string GameObjectName = "AlmediaLink";

        private static INativeBridge _instance;
        private static AlmediaLinkBridge _bridge;

        internal static AlmediaLinkBridge Bridge => _bridge;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnDomainReload()
        {
            _instance = null;
            _bridge = null;
        }

        internal static INativeBridge Create()
        {
            if (_instance != null && _bridge != null && _bridge.gameObject != null)
                return _instance;

            var go = new GameObject(GameObjectName);
            Object.DontDestroyOnLoad(go);
            _bridge = go.AddComponent<AlmediaLinkBridge>();

#if UNITY_EDITOR
            _instance = new EditorMockBridge(_bridge);
#elif UNITY_IOS
            _instance = new iOSNativeBridge();
#elif UNITY_ANDROID
            _instance = new AndroidNativeBridge();
#else
            AlmediaLog.Error("Unsupported platform. SDK will not function.");
            _instance = new EditorMockBridge(_bridge);
#endif
            return _instance;
        }
    }
}
