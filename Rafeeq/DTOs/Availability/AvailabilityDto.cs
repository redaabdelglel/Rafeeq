namespace Rafeeq.DTOs.Availability
{
        public class AvailabilityDto
        {
            public int? AvailabilityId { get; set; } // Non-nullable
            public string? DayOfWeek { get; set; } // Non-nullable
            public TimeSpan? StartTime { get; set; } // Non-nullable
            public TimeSpan? EndTime { get; set; } // Non-nullable
        }
    
}
