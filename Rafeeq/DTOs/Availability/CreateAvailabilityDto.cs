using System;
using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Availability
{
    public class CreateAvailabilityDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(0, 6, ErrorMessage = "Day of week must be between 0 (Sunday) and 6 (Saturday)")]
        public int DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }
    }
}
