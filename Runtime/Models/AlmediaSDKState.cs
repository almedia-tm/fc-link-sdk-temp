namespace AlmediaLink.Models
{
    public class AlmediaSDKState
    {
        public bool IsEnabled { get; }
        public bool IsAvailable { get; }
        public bool IsLinked { get; }
        public string LinkedAt { get; }

        public AlmediaSDKState(bool isEnabled, bool isAvailable, bool isLinked, string linkedAt)
        {
            IsEnabled = isEnabled;
            IsAvailable = isAvailable;
            IsLinked = isLinked;
            LinkedAt = linkedAt;
        }

        internal static AlmediaSDKState FromStatus(AlmediaStatus almediaStatus)
        {
            // Disabled is the only status that means the integration itself is off
            // (backend killswitch). Blocked is per-user, the integration is still
            // enabled, this user just cannot participate.
            bool isEnabled = almediaStatus != AlmediaStatus.Disabled;
            bool isAvailable = almediaStatus == AlmediaStatus.Eligible || almediaStatus == AlmediaStatus.Linked;
            bool isLinked = almediaStatus == AlmediaStatus.Linked;
            return new AlmediaSDKState(isEnabled, isAvailable, isLinked, "");
        }
    }
}
