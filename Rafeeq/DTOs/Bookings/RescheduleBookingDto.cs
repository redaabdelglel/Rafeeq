using System;
using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Bookings
{
    public class RescheduleBookingDto
    {
        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime EndDateTime { get; set; }
    }
}
