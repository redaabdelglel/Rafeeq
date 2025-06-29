using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Rafeeq.Models
{
    public class FAQ
    {
        [Key]
        public int FAQId { get; set; }

        [Required]
        [StringLength(500)]
        public string Question { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Answer { get; set; }

        [StringLength(100)]
        public string? Category { get; set; } 

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public int ViewCount { get; set; } = 0;

        public int HelpfulCount { get; set; } = 0;
        public int NotHelpfulCount { get; set; } = 0;

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; }
    }
}
