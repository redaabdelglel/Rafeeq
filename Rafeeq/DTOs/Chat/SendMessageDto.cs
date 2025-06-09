using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Chat
{
    public class SendMessageDto
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 2000 characters")]
        public string MessageText { get; set; }
    }
}
