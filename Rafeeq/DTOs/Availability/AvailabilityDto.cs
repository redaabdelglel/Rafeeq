using System;

namespace Rafeeq.DTOs.Availability
{
    public class AvailabilityDto
    {
        public int AvailabilityId { get; set; }
        public int UserId { get; set; }
        public int DayOfWeek { get; set; } // 0 = Sunday, 6 = Saturday
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string DayName => Enum.GetName(typeof(DayOfWeek), DayOfWeek);
    }
}
