namespace AlmediaLink.UI
{
    internal static class TimeFormatUtils
    {
        internal static string FormatRelativeTime(string isoTimestamp)
        {
            if (string.IsNullOrEmpty(isoTimestamp))
                return "";

            if (!System.DateTime.TryParse(isoTimestamp, null,
                    System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
                return isoTimestamp;

            var elapsed = System.DateTime.UtcNow - dt.ToUniversalTime();

            if (elapsed.TotalSeconds < 60) return "now";
            if (elapsed.TotalMinutes < 60) return $"{(int)elapsed.TotalMinutes}min ago";
            if (elapsed.TotalHours < 24) return $"{(int)elapsed.TotalHours}h ago";
            if (elapsed.TotalDays < 30) return $"{(int)elapsed.TotalDays}d ago";
            if (elapsed.TotalDays < 365) return $"{(int)(elapsed.TotalDays / 30)}mo ago";
            return $"{(int)(elapsed.TotalDays / 365)}y ago";
        }
    }
}
