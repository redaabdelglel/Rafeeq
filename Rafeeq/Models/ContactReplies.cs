using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Rafeeq.Models
{
    public class ContactReplies
    {
        [Key]
        public int ReplyId { get; set; }

        [Required]
        public int MessageId { get; set; }

        [ForeignKey("MessageId")]
        public ContactMessage Message { get; set; }

        public int? ResponderId { get; set; }  

        [ForeignKey("ResponderId")]
        public User Responder { get; set; }

        [Required]
        public string ReplyText { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}

