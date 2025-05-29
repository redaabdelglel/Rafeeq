namespace Rafeeq.Models
{
    public class MenteeCV
    {
        public int CVId { get; set; }
        public int UserId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public int FileSize { get; set; }
        public string ContentType { get; set; }
        public DateTime UploadDate { get; set; }
        public bool IsActive { get; set; }

        // Navigation properties
        public User User { get; set; }
        public ICollection<CVComment> Comments { get; set; }
    }
}
