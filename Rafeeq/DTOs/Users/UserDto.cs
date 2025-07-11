
namespace Rafeeq.DTOs.Users
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Bio { get; set; }
        public bool? IsMentor { get; set; }
        public bool? IsInterviewer { get; set; }
        public decimal? HourlyRate { get; set; }
    }
}
