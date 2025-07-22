using Rafeeq.DTOs.Chat;

public class ChatConversationDto
{
    public int ConversationId { get; set; }
    public int BookingId { get; set; }
    public int MentorId { get; set; }
    public string MentorName { get; set; }
    public string MentorProfilePicture { get; set; }
    public int MenteeId { get; set; }
    public string MenteeName { get; set; }
    public string MenteeProfilePicture { get; set; }
    public DateTime LastMessageAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public ChatMessageDto LastMessage { get; set; }
    public int UnreadCount { get; set; }
    public string SessionType { get; set; } 
    public string SessionStatus { get; set; } 
}
