using Rafeeq.DTOs.Skills;

using Rafeeq.DTOs.Availability;

namespace Rafeeq.DTOs.Users
{
    public class MentorDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
       public string role { get; set; }
        public decimal HourlyRate { get; set; }
        public List<SkillDto> MentorSkills { get; internal set; }
        public int UserId { get; set; }
  
        public string ProfilePicture { get; set; }
        public string Bio { get; set; }
        public List<string> Skills { get; set; }
        public List<AvailabilityDto> Availabilities { get; set; } = new List<AvailabilityDto>();
        public bool IsMentor { get; set; }
        public bool? IsInterviewer { get; set; }


    }
}
