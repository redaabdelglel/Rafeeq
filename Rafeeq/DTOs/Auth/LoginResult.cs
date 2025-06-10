namespace Rafeeq.DTOs.Auth
{
    public class LoginResult
    {
        public TokenResponseDto? TokenData { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsSuccess => TokenData != null;
    }
}
