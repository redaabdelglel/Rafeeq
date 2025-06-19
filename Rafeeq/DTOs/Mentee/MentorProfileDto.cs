using Rafeeq.DTOs.Availability;
using Rafeeq.DTOs.Reviews;

namespace Rafeeq.DTOs.Mentee
{
    public class MentorProfileDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Bio { get; set; }
        public string ProfilePicture { get; set; }
        public decimal? HourlyRate { get; set; }
        public double? AverageRating { get; set; }
        public List<MentorSkillDto> Skills { get; set; }
        public List<AvailabilityDto> Availabilities { get; set; }
        public List<ReviewDto> Reviews { get; set; }
    }

    public class MentorSkillDto
    {
        public int SkillId { get; set; }
        public string SkillName { get; set; }
    }

}
