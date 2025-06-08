namespace Rafeeq.DTOs.Bookings
{
    public class BookingDetailsDTO : BookingDto
    {
        public int MenteeId { get; set; }
        public string MenteeName { get; set; }
        public string PaymentStatus { get; set; }
        public decimal? TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
