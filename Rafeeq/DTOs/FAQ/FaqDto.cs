namespace Rafeeq.DTOs.FAQ
{
    public class FaqDto
    {
        public int FaqId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int SortOrder { get; set; }
        public int ViewCount { get; set; } 
    }
}
