namespace Rafeeq.DTOs.CV
{
    public class CVCommentDto
    {
        public int CommentId { get; set; }
        public int CVId { get; set; }
        public int MentorId { get; set; }
        public string MentorName { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
