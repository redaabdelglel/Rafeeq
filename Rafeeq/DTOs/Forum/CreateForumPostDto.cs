namespace Rafeeq.DTOs.Forum
{
    public class CreateForumPostDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public int CategoryId { get; set; }
    }
}
