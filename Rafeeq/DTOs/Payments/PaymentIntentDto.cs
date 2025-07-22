using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Payments
{
    public class PaymentIntentDto
    {
        [Required]
        public int BookingId { get; set; }

        
        public string ClientSecret { get; set; }
        public string PaymentIntentId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
    }
}
