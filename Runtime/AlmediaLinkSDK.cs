using System;
using System.Collections.Generic;
using UnityEngine;
using AlmediaLink.Bridge;
using AlmediaLink.Models;
using AlmediaLink.UI;

namespace AlmediaLink
{
    public static class AlmediaLinkSDK
    {
        public static string Version => "0.2.0-preview.2";

        public static event Action<AlmediaSDKState> OnInitialized;
        public static event Action<AlmediaStatus> OnStatusChanged;
        public static event Action<string> OnLinkCompleted;
        public static event Action<List<AlmediaNotification>> OnNotifications;
        public static event Action<AlmediaError> OnError;

        public static event Action<AlmediaLogLevel, string> OnLog
        {
            add => AlmediaLog.OnLog += value;
            remove => AlmediaLog.OnLog -= value;
        }

        private static INativeBridge _bridge;
        private static AlmediaStatus _almediaStatus = AlmediaStatus.NotInitialized;
        private static bool _initResultFired;

        public static void Initialize(AlmediaLinkConfig config)
        {
            AlmediaLog.Info($"Initializing SDK v{Version}");

            var resolved = config.Resolve();

            if (!resolved.IsValid)
            {
                AlmediaLog.Error("Integration key is missing. Cannot initialize.");
                OnError?.Invoke(new AlmediaError(
                    AlmediaErrorCode.InvalidConfiguration,
                    "Integration key is missing.", 0));
                return;
            }

            _initResultFired = false;
            _almediaStatus = AlmediaStatus.NotInitialized;

            _bridge = NativeBridgeFactory.Create();
            SubscribeToBridge();

            var request = InitializeRequest.FromResolvedConfig(resolved, Version);
            var json = JsonUtility.ToJson(request);
            _bridge.Initialize(json);

            AlmediaLinkUIManager.Initialize();

            AlmediaLog.Info("SDK initialized. Waiting for native callback.");
        }

        public static void StartLinking(PlacementType placement = PlacementType.Popup)
        {
            if (!GuardInitialized()) return;
            AlmediaLog.Info($"Starting link flow (placement: {placement})");
            _bridge.StartLinking(placement);
        }

        public static void FetchNotifications()
        {
            if (!GuardInitialized()) return;
            _bridge.FetchNotifications();
        }

        public static void StartNotificationPolling()
        {
            if (!GuardInitialized()) return;
            _bridge.StartNotificationPolling();
        }

        public static void StopNotificationPolling()
        {
            if (!GuardInitialized()) return;
            _bridge.StopNotificationPolling();
        }

        internal static void ContinueWithATT()
        {
            if (!GuardInitialized()) return;
            _bridge.ContinueWithATT();
        }

        internal static void SkipATT()
        {
            if (!GuardInitialized()) return;
            _bridge.SkipATT();
        }

        internal static void TrackPromoLoad(PromoState state)
        {
            if (!GuardInitialized()) return;
            _bridge.TrackPromoLoad(state);
        }

        internal static void TrackPromoClick()
        {
            if (!GuardInitialized()) return;
            _bridge.TrackPromoClick();
        }

        internal static void TrackPopupShow()
        {
            if (!GuardInitialized()) return;
            _bridge.TrackPopupShow();
        }

        internal static void TrackPopupDismiss()
        {
            if (!GuardInitialized()) return;
            _bridge.TrackPopupDismiss();
        }

        internal static void TrackPopupCtaClick()
        {
            if (!GuardInitialized()) return;
            _bridge.TrackPopupCtaClick();
        }

        internal static void TrackNotificationsShow(string notificationIdsJson)
        {
            if (!GuardInitialized()) return;
            _bridge.TrackNotificationsShow(notificationIdsJson);
        }

        internal static void TrackNotificationClick(string notificationId)
        {
            if (!GuardInitialized()) return;
            _bridge.TrackNotificationClick(notificationId);
        }

        internal static void TrackATTPreliminaryShow()
        {
            if (!GuardInitialized()) return;
            _bridge.TrackATTPreliminaryShow();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnDomainReload()
        {
            OnInitialized = null;
            OnStatusChanged = null;
            OnLinkCompleted = null;
            OnNotifications = null;
            OnError = null;
            AlmediaLog.ClearSubscribers();
            AlmediaLinkUIManager.Cleanup();
            _bridge = null;
            _initResultFired = false;
            _almediaStatus = AlmediaStatus.NotInitialized;
        }

        private static void HandleStatusChanged(StatusChangedResponse response)
        {
            _almediaStatus = StatusExtensions.FromString(response.status);
            AlmediaLog.Info($"Status changed: {_almediaStatus}");
            OnStatusChanged?.Invoke(_almediaStatus);

            if (!_initResultFired && IsTerminalInitStatus(_almediaStatus))
            {
                _initResultFired = true;
                var state = AlmediaSDKState.FromStatus(_almediaStatus);
                AlmediaLog.Info($"Initialization complete — available: {state.IsAvailable}, linked: {state.IsLinked}");
                OnInitialized?.Invoke(state);
            }
        }

        private static void HandleLinkCompleted(LinkCompletedResponse response)
        {
            AlmediaLog.Info($"Link completed at {response.linkedAt}");
            OnLinkCompleted?.Invoke(response.linkedAt);
        }

        private static void HandleNotificationsReceived(NotificationsReceivedResponse response)
        {
            if (response.notifications == null || response.notifications.Length == 0) return;
            AlmediaLog.Debug($"Received {response.notifications.Length} notification(s)");
            
            var list = new List<AlmediaNotification>(response.notifications.Length);
            
            foreach (var item in response.notifications)
            {
                list.Add(AlmediaNotification.FromNotificationItem(item));
            }
            
            OnNotifications?.Invoke(list);
        }

        private static void HandleErrorOccurred(ErrorCallbackResponse response)
        {
            AlmediaLog.Error($"Error from native: {response.code} — {response.message}");
            OnError?.Invoke(AlmediaError.FromCallback(response));
        }

        private static void HandleShowATTPrePrompt()
        {
            AlmediaLog.Info("Native requested ATT pre-prompt.");
            AlmediaLinkUIManager.ShowATTPrePrompt();
        }

        private static bool IsTerminalInitStatus(AlmediaStatus status)
        {
            return status == AlmediaStatus.Eligible || status == AlmediaStatus.Linked
                || status == AlmediaStatus.NotAvailable || status == AlmediaStatus.Blocked
                || status == AlmediaStatus.Disabled;
        }

        private static bool GuardInitialized()
        {
            if (_bridge == null)
            {
                AlmediaLog.Warning("SDK not initialized. Call Initialize() first.");
                return false;
            }
            return true;
        }

        private static void SubscribeToBridge()
        {
            UnsubscribeFromBridge();
            AlmediaLinkBridge.StatusChanged += HandleStatusChanged;
            AlmediaLinkBridge.LinkCompleted += HandleLinkCompleted;
            AlmediaLinkBridge.NotificationsReceived += HandleNotificationsReceived;
            AlmediaLinkBridge.ErrorOccurred += HandleErrorOccurred;
            AlmediaLinkBridge.ShowATTPrePromptRequested += HandleShowATTPrePrompt;
        }

        private static void UnsubscribeFromBridge()
        {
            AlmediaLinkBridge.StatusChanged -= HandleStatusChanged;
            AlmediaLinkBridge.LinkCompleted -= HandleLinkCompleted;
            AlmediaLinkBridge.NotificationsReceived -= HandleNotificationsReceived;
            AlmediaLinkBridge.ErrorOccurred -= HandleErrorOccurred;
            AlmediaLinkBridge.ShowATTPrePromptRequested -= HandleShowATTPrePrompt;
        }
    }
}
