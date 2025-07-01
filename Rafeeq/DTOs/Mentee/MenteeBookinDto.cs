namespace Rafeeq.DTOs.Mentee
{
    public class MenteeBookingDto
    {
        public int BookingId { get; set; }
        public string MentorName { get; set; }
        public string SessionType { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Status { get; set; }
        public string GoogleMeetLink { get; set; }
    }

    public class MenteeBookingDetailsDto : MenteeBookingDto
    {
        public string MentorProfilePicture { get; set; }
        public string MentorBio { get; set; }
        public string MentorSpecialization { get; set; }
        public string MeetingAgenda { get; set; }
        public string PaymentStatus { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
