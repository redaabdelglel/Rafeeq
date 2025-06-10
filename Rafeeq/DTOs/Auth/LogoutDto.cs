using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Auth
{
    public class LogoutDto
    {
        [Required(ErrorMessage = "Refresh token is required.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
