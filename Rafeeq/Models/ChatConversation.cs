using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rafeeq.Models
{
    public class ChatConversation
    {
        [Key]
        public int ConversationId { get; set; }

        public int? BookingId { get; set; }
        public int? MentorId { get; set; }
        public int? MenteeId { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? LastMessageAt { get; set; }

        public bool? IsActive { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedAt { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }

        [ForeignKey("MentorId")]
        public virtual User Mentor { get; set; }

        [ForeignKey("MenteeId")]
        public virtual User Mentee { get; set; }

        [InverseProperty("Conversation")]
        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
