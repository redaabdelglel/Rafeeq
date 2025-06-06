namespace Rafeeq.DTOs.Mentee
{
    public class MenteeDashboardDto
    {
        public string MenteeName { get; set; }
        public DashboardStatsDto Stats { get; set; }
        public List<UpcomingSessionDto> UpcomingSessions { get; set; }
        public List<RecentActivityDto> RecentActivities { get; set; }
    }
}
