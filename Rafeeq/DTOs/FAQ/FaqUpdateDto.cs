using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.FAQ
{
    public class FaqUpdateDto
    {
        [Required]
        [StringLength(500)]
        public string Question { get; set; } = string.Empty;
        [Required]
        public string Answer { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Category { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
