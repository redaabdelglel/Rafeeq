namespace Rafeeq.DTOs.CV
{
    public class CVDTO
    {
        public int CVId { get; set; }
        public int UserId { get; set; }
        public string FileName { get; set; }
        public int FileSize { get; set; }
        public DateTime UploadDate { get; set; }
        public string DownloadUrl { get; set; }
    }
}
