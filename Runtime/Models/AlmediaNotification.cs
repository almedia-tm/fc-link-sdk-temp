namespace AlmediaLink.Models
{
    public class AlmediaNotification
    {
        public string Id { get; }
        public string Title { get; }
        public string Message { get; }
        public string Timestamp { get; }
        public string Type { get; }

        public AlmediaNotification(string id, string title, string message, string timestamp, string type)
        {
            Id = id;
            Title = title;
            Message = message;
            Timestamp = timestamp;
            Type = type;
        }

        internal static AlmediaNotification FromNotificationItem(NotificationItem item)
        {
            return new AlmediaNotification(item.id, item.title, item.message, item.timestamp, item.type);
        }
    }
}
