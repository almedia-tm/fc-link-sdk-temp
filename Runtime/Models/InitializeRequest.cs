using UnityEngine;

namespace AlmediaLink.Models
{
    [System.Serializable]
    internal class InitializeRequest
    {
        public string integrationKey;
        public string accountId;
        public string gaid;
        public string oaid;
        public string idfa;
        public string adid;
        public string afid;
        public int notificationsPollingIntervalSec;
        public bool canRunConsentFlow;

        // TEMP-ATT-DISMISSAL-CFG
        public int attPromptMaxDismissals;

        // TEMP-ATT-DISMISSAL-CFG
        public double attPromptRetryIntervalHours;

        public string sdkVersion;

        public static InitializeRequest FromResolvedConfig(ResolvedAlmediaLinkConfig config, string sdkVersion)
        {
            return new InitializeRequest
            {
                integrationKey = config.IntegrationKey ?? "",
                accountId = config.AccountId ?? "",
                gaid = config.Gaid ?? "",
                oaid = config.Oaid ?? "",
                idfa = config.Idfa ?? "",
                adid = config.AdjustDeviceId ?? "",
                afid = config.AppsFlyerId ?? "",
                notificationsPollingIntervalSec = config.NotificationsPollingIntervalSec,
                canRunConsentFlow = config.CanRunConsentFlow,
                // TEMP-ATT-DISMISSAL-CFG
                attPromptMaxDismissals = config.AttPromptMaxDismissals,
                // TEMP-ATT-DISMISSAL-CFG
                attPromptRetryIntervalHours = config.AttPromptRetryIntervalHours,
                sdkVersion = sdkVersion ?? "",
            };
        }
    }
}
