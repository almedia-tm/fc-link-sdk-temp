namespace AlmediaLink
{
    /// <summary>
    /// The result of merging an AlmediaLinkConfig with AlmediaLinkSettings defaults.
    /// Contains final resolved values ready for the native bridge.
    /// </summary>
    public class ResolvedAlmediaLinkConfig
    {
        public bool IsValid { get; internal set; }

        public string IntegrationKey { get; internal set; }
        public string Gaid { get; internal set; }
        public string Oaid { get; internal set; }
        public string Idfa { get; internal set; }
        public string AdjustDeviceId { get; internal set; }
        public string AppsFlyerId { get; internal set; }
        public string AccountId { get; internal set; }
        public int NotificationsPollingIntervalSec { get; internal set; }
        public bool CanRunConsentFlow { get; internal set; }

        // TEMP-ATT-DISMISSAL-CFG
        public int AttPromptMaxDismissals { get; internal set; }

        // TEMP-ATT-DISMISSAL-CFG
        public double AttPromptRetryIntervalHours { get; internal set; }
    }
}
