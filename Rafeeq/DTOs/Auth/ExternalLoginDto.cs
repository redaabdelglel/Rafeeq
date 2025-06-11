using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Auth
{
    public class ExternalLoginDto
    {
        [Required(ErrorMessage = "Provider is required.")]
        public string Provider { get; set; } = string.Empty; 

        [Required(ErrorMessage = "ID Token is required.")]
        public string IdToken { get; set; } = string.Empty; 

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Profile picture URL cannot exceed 255 characters.")]
        public string? ProfilePicture { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        [RegularExpression("^(Mentee|Mentor)$", ErrorMessage = "Role must be 'Mentee' or 'Mentor'.")]
        public string Role { get; set; } = string.Empty; 
    }
}
