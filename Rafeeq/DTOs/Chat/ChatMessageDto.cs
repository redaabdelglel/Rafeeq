using System;
using System.Collections.Generic;

namespace Rafeeq.DTOs.Chat
{
    public class ChatMessageDto
    {
        public int MessageId { get; set; }
        public int BookingId { get; set; }
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; }
        public string ProfilePicture { get; set; }
        public string MessageText { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
        public List<string> ReadByUserIds { get; set; } = new List<string>();
        public List<ChatAttachmentDto> Attachments { get; set; } = new List<ChatAttachmentDto>();
    }
}
