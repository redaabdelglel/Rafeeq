namespace Rafeeq.DTOs.Auth
{
    public class AuthResponseDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; 
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiration { get; set; }
        public bool IsMentor { get; set; }
        public bool IsInterviewer { get; set; }
        public bool IsEmailVerified { get; set; }
    }
}
