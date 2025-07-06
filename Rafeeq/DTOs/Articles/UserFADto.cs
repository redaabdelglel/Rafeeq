namespace Rafeeq.DTOs.Articles
{
    public class UserFADto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Bio { get; set; }
        public bool? IsEmailVerified { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsMentor { get; set; }
        public bool? IsInterviewer { get; set; }
        public bool? IsDeleted { get; set; }
        public decimal? HourlyRate { get; set; }
    }
}
