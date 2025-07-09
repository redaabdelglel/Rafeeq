using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rafeeq.Models
{
    public class ForumPostUpvote
    {
        [Key]
        public int UpvoteId { get; set; }

        [Required]
        public int PostId { get; set; }

        [Required]
        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation
        [ForeignKey("PostId")]
        public ForumPost Post { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
