namespace Rafeeq.DTOs.Bookings
{
    public class BookingDTO
    {
        public int BookingId { get; set; }
        public int MentorId { get; set; }
        public string MentorName { get; set; }
        public string SessionType { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Status { get; set; }
        public string GoogleMeetLink { get; set; }
    }
}
