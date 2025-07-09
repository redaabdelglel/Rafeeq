namespace Rafeeq.DTOs.ForumComment
{
    public class ForumPostDto
    {
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserProfilePicture { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsSolved { get; set; }
        public int Upvotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<ForumCommentDto> Comments { get; set; } = new List<ForumCommentDto>();

        public bool HasUpvoted { get; set; }
        public bool CanEditDelete { get; set; }
        public bool CanMarkAsSolved { get; set; }
    }
}
