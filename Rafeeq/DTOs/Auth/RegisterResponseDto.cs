namespace Rafeeq.DTOs.Auth
{
    public class RegisterResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsEmailAlreadyRegistered { get; set; }
    }
}
