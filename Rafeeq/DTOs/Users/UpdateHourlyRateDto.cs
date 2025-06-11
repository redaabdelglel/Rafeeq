using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Users
{
    public class UpdateHourlyRateDto
    {
        [Required]
        [Range(0.01, 1000.00, ErrorMessage = "Hourly rate must be between 0.01 and 1000.00")]
        public decimal HourlyRate { get; set; }
    }
}
