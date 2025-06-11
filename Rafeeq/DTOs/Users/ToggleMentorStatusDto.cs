using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Users
{
    public class ToggleMentorStatusDto
    {
        [Required]
        public bool IsInterviewer { get; set; }
    }
}
