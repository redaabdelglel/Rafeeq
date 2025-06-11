using Rafeeq.DTOs.CV;

namespace Rafeeq.DTOs.Users
{
    public class MenteeDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ProfilePicture { get; set; }
        public string Bio { get; set; }
        public bool? IsEmailVerified { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<string> Skills { get; set; } = new List<string>();
        public List<MenteeCVDto> CVs { get; set; } = new List<MenteeCVDto>();
    }
}
