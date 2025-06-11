using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Users
{
    public class UpdateProfileDto
    {
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        public string? FullName { get; set; }

        public string? ProfilePicture { get; set; }
        public string? Bio { get; set; }
        public decimal? HourlyRate { get; set; }
        public bool? IsInterviewer { get; set; }
    }
}
