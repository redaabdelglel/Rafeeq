using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Payments
{
    public class PaymentIntentDto
    {
        [Required]
        public int BookingId { get; set; }

        // These properties will be populated by the server, not required from client
        public string ClientSecret { get; set; }
        public string PaymentIntentId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
    }
}
