using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rafeeq.Models
{
    public class MessageReadStatus
    {
        [Key]
        public int ReadStatusId { get; set; }

        public int? MessageId { get; set; }
        public int? UserId { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? ReadAt { get; set; }

        [ForeignKey("MessageId")]
        public virtual ChatMessage Message { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
