using Rafeeq.DTOs.Availability;
using Rafeeq.DTOs.Skills;

namespace Rafeeq.DTOs.Users
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public string? Bio { get; set; }
        public bool IsEmailVerified { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsMentor { get; set; }
        public bool? IsInterviewer { get; set; }
        public decimal? HourlyRate { get; set; }
        public List<SkillDto>? MentorSkills { get; set; }
        public List<SkillDto>? MenteeSkills { get; set; }
        public List<string>? Skills { get; set; } 
        public List<AvailabilityDto>? Availabilities { get; set; }
    }
}
