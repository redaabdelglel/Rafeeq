namespace Rafeeq.DTOs.Availability
{
    public class AvailableSlotDto
    {
        public int DayOfWeek { get; set; }
        public string DayName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public List<DateTime> AvailableSlots { get; set; } = new List<DateTime>();
        public string TimeRange =>
            $"{DateTime.Today.Add(StartTime):h\\:mm tt} - {DateTime.Today.Add(EndTime):h\\:mm tt}";
    }

    public class TimeSlotDto
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Formatted => $"{Start:ddd, MMM dd} • {Start:h:mm tt} - {End:h:mm tt}";
    }
}
