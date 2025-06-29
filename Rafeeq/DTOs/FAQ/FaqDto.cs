namespace Rafeeq.DTOs.FAQ
{
    public class FaqDto
    {
        public int FAQId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int ViewCount { get; set; }
        public int HelpfulCount { get; set; } 
        public int NotHelpfulCount { get; set; } 
        public DateTime CreatedAt { get; set; }
    }
}
