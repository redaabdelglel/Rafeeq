using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Payments
{
    public class PaymentConfirmationDto
    {
        [Required]
        public string PaymentIntentId { get; set; }

        [Required]
        public int BookingId { get; set; }

        [Required]
        public int PaymentId { get; set; }
    }
}
