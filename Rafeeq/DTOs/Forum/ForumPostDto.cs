namespace Rafeeq.DTOs.Forum
{
    public class ForumPostDto
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsSolved { get; set; }
        public int Upvotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; }
    }
}
