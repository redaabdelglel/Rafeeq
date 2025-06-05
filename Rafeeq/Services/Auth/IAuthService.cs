using Rafeeq.DTOs.Auth;

namespace Rafeeq.Services.Auth
{
    public interface IAuthService
    {
        Task<TokenResponseDto?> RegisterAsync(RegisterDto dto);
        Task<TokenResponseDto?> LoginAsync(LoginDto dto);
        Task<TokenResponseDto?> ExternalLoginAsync(ExternalLoginDto dto);
        Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken);
        Task ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<bool> VerifyEmailAsync(string token);
        Task ResendVerificationEmailAsync(string email);
        Task<bool> InvalidateRefreshTokenAsync(int userId); // For server-side refresh token invalidation
    }
}
