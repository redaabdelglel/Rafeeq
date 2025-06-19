
using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Skills
{
    public class AddSkillDto
    {
        [Required]
        public int SkillId { get; set; }
    }
}
