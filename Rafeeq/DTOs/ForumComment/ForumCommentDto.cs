namespace Rafeeq.DTOs.ForumComment
{
    public class ForumCommentDto
    {
        public int CommentId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserProfilePicture { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsAnswer { get; set; } 
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; } 

        public bool CanEditDelete { get; set; }
    }
}
