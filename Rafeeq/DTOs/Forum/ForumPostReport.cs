public class ForumPostReportDto
{
    public int ReportId { get; set; }
    public int PostId { get; set; }
    public int ReportedByUserId { get; set; }
    public string Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; }
    public string? AdminNote { get; set; }
    public string PostTitle { get; set; }
    public string? PostOwnerName { get; set; }
    public string ReportedByUserName { get; set; }
}
