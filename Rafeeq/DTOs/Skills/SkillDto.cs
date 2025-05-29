
namespace Rafeeq.DTOs.Skills
{
    public class SkillDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CreateSkillDto
    {
        public string Name { get; set; }
    }

    public class UpdateSkillDto
    {
        public string Name { get; set; }
    }
}
