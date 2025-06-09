namespace Rafeeq.DTOs.Payments
{
    public class PaymentDto
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionId { get; set; }
        public DateTime PaymentDate { get; set; }

        public string MenteeFullName { get; set; }
        public string MentorFullName { get; set; }
    }
}
