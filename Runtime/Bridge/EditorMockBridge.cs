#if UNITY_EDITOR
using System.Collections;
using AlmediaLink.Models;
using UnityEngine;

namespace AlmediaLink.Bridge
{
    internal class EditorMockBridge : INativeBridge
    {
        private const float StatusTransitionDelay = 0.1f;
        private const float LinkFlowDelay = 0.5f;
        private const float PollDelay = 0.2f;
        private const float ATTDelay = 0.3f;

        private readonly MonoBehaviour _host;

        public EditorMockBridge(MonoBehaviour host)
        {
            _host = host;
        }

        public void Initialize(string json)
        {
            AlmediaLog.Debug("Editor mock: Initialize");

            // Check if ATT pre-prompt should be shown
            var request = JsonUtility.FromJson<InitializeRequest>(json);
            if (request.canRunConsentFlow)
            {
                AlmediaLog.Debug("Editor mock: canRunConsentFlow=true, showing ATT pre-prompt");
                _host.StartCoroutine(SimulateShowATTPrePrompt());
                return;
            }

            _host.StartCoroutine(SimulateInitialize());
        }

        public void StartLinking(PlacementType placement)
        {
            AlmediaLog.Debug($"Editor mock: StartLinking (placement={placement.ToNativeString()})");
            _host.StartCoroutine(SimulateLinkFlow());
        }

        public void FetchNotifications()
        {
            AlmediaLog.Debug("Editor mock: FetchNotifications");
            _host.StartCoroutine(SimulatePollNotifications());
        }

        public void StartNotificationPolling()
        {
            AlmediaLog.Debug("Editor mock: StartNotificationPolling");
        }

        public void StopNotificationPolling()
        {
            AlmediaLog.Debug("Editor mock: StopNotificationPolling");
        }

        public void ContinueWithATT()
        {
            AlmediaLog.Debug("Editor mock: ContinueWithATT");
            _host.StartCoroutine(SimulateInitialize());
        }

        public void SkipATT()
        {
            AlmediaLog.Debug("Editor mock: SkipATT");
            _host.StartCoroutine(SimulateInitialize());
        }

        public void TrackPromoLoad(PromoState state) => AlmediaLog.Debug($"Editor mock: TrackPromoLoad state={state.ToNativeString()}");
        public void TrackPromoClick() => AlmediaLog.Debug("Editor mock: TrackPromoClick");
        public void TrackPopupShow() => AlmediaLog.Debug("Editor mock: TrackPopupShow");
        public void TrackPopupDismiss() => AlmediaLog.Debug("Editor mock: TrackPopupDismiss");
        public void TrackPopupCtaClick() => AlmediaLog.Debug("Editor mock: TrackPopupCtaClick");
        public void TrackNotificationsShow(string notificationIdsJson) => AlmediaLog.Debug($"Editor mock: TrackNotificationsShow {notificationIdsJson}");
        public void TrackNotificationClick(string notificationId) => AlmediaLog.Debug($"Editor mock: TrackNotificationClick id={notificationId}");
        public void TrackATTPreliminaryShow() => AlmediaLog.Debug("Editor mock: TrackATTPreliminaryShow");

        private IEnumerator SimulateShowATTPrePrompt()
        {
            yield return new WaitForSeconds(ATTDelay);
            _host.gameObject.SendMessage("ShowATTPrePrompt", "");
        }

        private IEnumerator SimulateInitialize()
        {
            yield return new WaitForSeconds(StatusTransitionDelay);
            SendStatusChanged("eligible");
        }

        private IEnumerator SimulateLinkFlow()
        {
            yield return new WaitForSeconds(LinkFlowDelay);
            SendStatusChanged("linked");

            yield return new WaitForSeconds(StatusTransitionDelay);
            var json = JsonUtility.ToJson(new LinkCompletedResponse
            {
                linkedAt = System.DateTime.UtcNow.ToString("o")
            });
            _host.gameObject.SendMessage("OnLinkCompleted", json);
        }

        private IEnumerator SimulatePollNotifications()
        {
            yield return new WaitForSeconds(PollDelay);
            var response = new NotificationsReceivedResponse
            {
                notifications = new[]
                {
                    new NotificationItem
                    {
                        id = "mock-1",
                        title = "Mock Reward",
                        message = "You earned a mock reward!",
                        timestamp = System.DateTime.UtcNow.AddMinutes(-5).ToString("o"),
                        type = "reward"
                    },
                    new NotificationItem
                    {
                        id = "mock-2",
                        title = "Level Complete",
                        message = "You completed level 3!",
                        timestamp = System.DateTime.UtcNow.AddHours(-1).ToString("o"),
                        type = "status"
                    },
                    new NotificationItem
                    {
                        id = "mock-3",
                        title = "Daily Bonus",
                        message = "Claim your $0.25 daily bonus!",
                        timestamp = System.DateTime.UtcNow.ToString("o"),
                        type = "reward"
                    }
                }
            };
            _host.gameObject.SendMessage("OnNotifications", JsonUtility.ToJson(response));
        }

        private void SendStatusChanged(string status)
        {
            var json = JsonUtility.ToJson(new StatusChangedResponse { status = status });
            _host.gameObject.SendMessage("OnStatusChanged", json);
        }
    }
}
#endif
