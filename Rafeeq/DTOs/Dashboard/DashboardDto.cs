namespace Rafeeq.DTOs.Dashboard
{
    public class DashboardDto
    {
        public int activeUsers { get; set; }
        public int totalUsers { get; set; }
        public decimal Revenue { get; set; }    
        public double UsersGrowthPrecentage { get; set; }
        public double RevenueGrowthPrecentage { get; set; }
        public int totalMentors { get; set; }
        public int totalMentees { get; set; }
        public int totalBookings { get; set; }
        public int totalSkills { get; set; }
        public List<int> MonthlyUserGrowth { get; set; }
        public List<decimal> MonthlyRevenue { get; set; }







    }
}
