using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Users
{
    public class UpdateMentorProfileDto
    {
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        public string? FullName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string? Email { get; set; }

        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must contain at least 8 characters, one uppercase, one lowercase, one digit, and one special character.")]
        public string? Password { get; set; }

        [Url(ErrorMessage = "Invalid URL format for profile picture.")]
        [StringLength(255, ErrorMessage = "Profile picture URL cannot exceed 255 characters.")]
        public string? ProfilePicture { get; set; }

        [StringLength(1000, ErrorMessage = "Bio cannot exceed 1000 characters.")]
        public string? Bio { get; set; }

        [Range(0.01, 1000.00, ErrorMessage = "Hourly rate must be between 0.01 and 1000.00.")]
        public decimal? HourlyRate { get; set; }

        public List<int>? SkillIds { get; set; } 

        public bool? IsInterviewer { get; set; }
    }
}
