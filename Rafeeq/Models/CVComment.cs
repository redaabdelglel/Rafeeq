namespace Rafeeq.Models
{
    public class CVComment
    {
        public int CommentId { get; set; }
        public int CVId { get; set; }
        public int MentorId { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

      
        public MenteeCV CV { get; set; }
        public User Mentor { get; set; }
    }
}
