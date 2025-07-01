namespace Rafeeq.DTOs.CV
{
    public class MenteeCVDto
    {
        public int CVId { get; set; }
        public int UserId { get; set; }
        public string FileName { get; set; }
        public DateTime UploadDate { get; set; }
        public bool IsActive { get; set; }
        public string UserFullName { get; set; } // Mentee name
        public List<CVCommentDto> Comments { get; set; }
        public string DownloadUrl { get; set; }
    }
}
