using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.CV
{
    public class UploadCVDTO
    {
        [Required]
        public IFormFile File { get; set; }

    }
}
