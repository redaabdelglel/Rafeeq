using Rafeeq.DTOs.Auth;
using Rafeeq.Models;
using System.Security.Claims;

namespace Rafeeq.Services.Auth
{
    public interface IJwtService
    {
        object GenerateToken(User admin);
        TokenResponseDto GenerateTokens(User user);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
