
using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Bookings
{
    public class UpdateMeetingLinkDto
    {
        [Required]
        [Url(ErrorMessage = "Please provide a valid meeting link")]
        public string MeetingLink { get; set; }
    }
}
