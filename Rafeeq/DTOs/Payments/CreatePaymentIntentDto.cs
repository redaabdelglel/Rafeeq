
using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Payments
{
    public class CreatePaymentIntentDto
    {
        [Required]
        public int BookingId { get; set; }
    }
}
