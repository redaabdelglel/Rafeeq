namespace Rafeeq.DTOs.Forum
{
    public class ForumCategoryDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int PostCount { get; set; }
    }
}
