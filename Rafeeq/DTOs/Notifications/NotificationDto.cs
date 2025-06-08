using System;

namespace Rafeeq.DTOs.Notifications
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public string Type { get; set; }
        public int? RelatedEntityId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
