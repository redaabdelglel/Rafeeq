namespace Rafeeq.DTOs.Mentee
{
    public class RecentActivityDto
    {
        public string ActivityType { get; set; } // "session", "review", "message", etc.
        public string Text { get; set; }
        public DateTime ActivityDate { get; set; }
    }
}
