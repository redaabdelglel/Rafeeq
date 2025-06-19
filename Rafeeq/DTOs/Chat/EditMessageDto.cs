
using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Chat
{
    public class EditMessageDto
    {
        [Required]
        public int MessageId { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 2000 characters")]
        public string MessageText { get; set; }
    }
}
