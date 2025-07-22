
namespace Rafeeq.DTOs.Mentee
{
    public class UpcomingSessionDto
    {
        public int BookingId { get; set; }
        public string MentorName { get; set; }
        public DateTime SessionDate { get; set; }
        public string SessionTime { get; set; }
        public string JoinUrl { get; set; }
        public string Status { get; set; }
        public string SessionType { get; set; }
        public string PaymentStatus { get; set; }
    }
}