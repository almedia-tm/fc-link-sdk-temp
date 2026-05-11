namespace AlmediaLink.Models
{
    public enum AlmediaStatus
    {
        NotInitialized,
        Eligible,
        Linked,
        NotAvailable,
        Blocked,
        Disabled
    }

    public static class StatusExtensions
    {
        public static AlmediaStatus FromString(string value)
        {
            switch (value)
            {
                case "notInitialized": return AlmediaStatus.NotInitialized;
                case "eligible": return AlmediaStatus.Eligible;
                case "linked": return AlmediaStatus.Linked;
                case "notAvailable": return AlmediaStatus.NotAvailable;
                case "blocked": return AlmediaStatus.Blocked;
                case "disabled": return AlmediaStatus.Disabled;
                default: return AlmediaStatus.NotInitialized;
            }
        }
    }
}
