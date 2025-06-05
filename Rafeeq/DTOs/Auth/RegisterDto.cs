using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Auth
{
    public class RegisterDto
    {

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters.")]
        public string FullName { get; set; } = string.Empty;



        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password is required.")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;


        [Required(ErrorMessage = "Role is required.")]
        [RegularExpression("^(Mentee|Mentor)$", ErrorMessage = "Role must be 'Mentee' or 'Mentor'.")]
        public string Role { get; set; } = string.Empty; // "Mentee" or "Mentor"
    }
}
 

