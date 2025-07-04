namespace Rafeeq.DTOs.Bookings
{
    public class UpdateBookingStatusDto
    {
        public decimal? TotalAmount { get; set; }
        public string Status { get; set; } // Pending, Confirmed, Completed, Cancelled
        public string GoogleMeetLink { get; set; }
    }
}
