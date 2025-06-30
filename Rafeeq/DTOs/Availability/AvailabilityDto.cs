using System;
using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Availability
{
    public class AvailabilityDto
    {
        public int AvailabilityId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Range(0, 6, ErrorMessage = "DayOfWeek must be between 0 (Sunday) and 6 (Saturday)")]
        public int DayOfWeek { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        public string DayName => Enum.GetName(typeof(DayOfWeek), DayOfWeek) ?? "Unknown";

        public bool IsValid() => StartTime < EndTime;

    }
}
