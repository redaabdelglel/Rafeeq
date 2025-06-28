
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rafeeq.Models
{
    [Table("MessageReactions")] 
    public class MessageReaction
    {
        [Key]
        public int ReactionId { get; set; }

        public int MessageId { get; set; }
        public int UserId { get; set; }

        [Required]
        [StringLength(10)]
        public string ReactionType { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; }

        [ForeignKey("MessageId")]
        public virtual ChatMessage Message { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
