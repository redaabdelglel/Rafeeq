using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rafeeq.Models
{
    public class ForumComment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        public int PostId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public bool IsAnswer { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation
        [ForeignKey("PostId")]
        public ForumPost Post { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
