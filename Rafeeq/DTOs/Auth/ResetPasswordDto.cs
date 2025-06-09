using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Auth
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Token is required.")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        //[Required(ErrorMessage = "Confirm new password is required.")]
        //[Compare("NewPassword", ErrorMessage = "Password and confirmation password do not match.")]
        //[DataType(DataType.Password)]
        //public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
