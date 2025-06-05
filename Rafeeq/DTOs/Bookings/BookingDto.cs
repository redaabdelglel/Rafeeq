namespace Rafeeq.DTOs.Bookings
{
    public class BookingDto
    {
        public int BookingId { get; set; }
        public string SessionType { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime ? EndDateTime { get; set; }
        public string Status { get; set; }
        public string GoogleMeetLink { get; set; }
        public string ?PaymentStatus { get; set; }
        public decimal ?TotalAmount { get; set; }
        public decimal? Commission { get; set; }
        public string MentorName { get; set; }
        public string MenteeName { get; set; }
    }
}
