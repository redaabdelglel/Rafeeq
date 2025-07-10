namespace Rafeeq.DTOs.Forum
{
    public class AdminReportActionDto
    {
        public string Action { get; set; } // "delete" or "ignore"
        public string? AdminNote { get; set; }
    }
}
