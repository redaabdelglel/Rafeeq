using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Rafeeq.Models
{
    public class ForumCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation
        public ICollection<ForumPost> Posts { get; set; }
    }
}
