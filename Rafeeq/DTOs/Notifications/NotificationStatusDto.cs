namespace Rafeeq.DTOs.Notifications
{
    public class NotificationStatusDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int UnreadCount { get; set; }
    }
}
