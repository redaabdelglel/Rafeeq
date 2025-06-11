// In Rafeeq/Models/ContactMessage.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rafeeq.Models
{
    [Table("ContactMessages")]
    public class ContactMessage
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(200)]
        public string Subject { get; set; }

        [Required]
        public string Message { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "New";

        public bool IsDeleted { get; set; } = false;

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime")]
        public DateTime? ResponseDate { get; set; }

        // Make ResponseMessage nullable
        public string? ResponseMessage { get; set; }

        public int? RespondedBy { get; set; }

        [ForeignKey("RespondedBy")]
        public virtual User? Responder { get; set; }
    }
}
