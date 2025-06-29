
public class MessageReactionInfoDto
{
    public int ReactionId { get; set; }
    public int MessageId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string ReactionType { get; set; }
    public DateTime CreatedAt { get; set; }
}
