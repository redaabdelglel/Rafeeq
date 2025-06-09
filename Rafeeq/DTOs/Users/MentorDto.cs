using Rafeeq.DTOs.Skills;

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



    }
}
