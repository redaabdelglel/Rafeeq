using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Chat
{
    public class MessageReactionDto
    {
        [Required]
        public int MessageId { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 1)]
        public string ReactionType { get; set; } // Emoji or string code like "like", "heart", etc.
    }
}