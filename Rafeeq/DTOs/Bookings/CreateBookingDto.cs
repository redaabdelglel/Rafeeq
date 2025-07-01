using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Bookings
{
    public class CreateBookingDTO
    {
        [Required]
        public int MentorId { get; set; }
        [Required]
        public string SessionType { get; set; }
        [Required]
        public DateTime StartDateTime { get; set; }
        [Required]
        public DateTime EndDateTime { get; set; }
        public decimal TotalAmount { get; set; }


    }
}
