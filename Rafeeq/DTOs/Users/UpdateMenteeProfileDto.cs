using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Users
{
    public class UpdateMenteeProfileDto
    {
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        public string? FullName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format.")]

        public string? Email { get; set; }

        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
           ErrorMessage = "Password must contain at least 8 characters, one uppercase, one lowercase, one digit, and one special character.")]
        public string? Password { get; set; }
        [Url(ErrorMessage = "Invalid URL format for profile picture.")]

        public string? ProfilePicture { get; set; }

        [StringLength(1000, ErrorMessage = "Bio cannot exceed 1000 characters.")] 

        public string? Bio { get; set; }
        public List<int>? SkillIds { get; set; }
    }
}
