using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.CV
{
    public class CreateCVCommentDTO
    {
        [Required]
        public int CVId { get; set; }
        [Required]
        public string Comment { get; set; }
    }
}
