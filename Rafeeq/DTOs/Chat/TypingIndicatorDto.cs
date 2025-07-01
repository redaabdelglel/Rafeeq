
using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Chat
{
    public class TypingIndicatorDto
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        public bool IsTyping { get; set; }
    }
}
