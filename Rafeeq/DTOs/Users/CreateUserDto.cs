using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Users
{
    public class CreateUserDto
    {

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } 

        [StringLength(255)]
        public string ProfilePicture { get; set; }

        public string Bio { get; set; }

        public string Role { get; set; } 

        public bool? IsMentor { get; set; }

        public bool? IsInterviewer { get; set; }

        public decimal? HourlyRate { get; set; }
    }




}

