using System;
using System.Collections.Generic;
using Rafeeq.DTOs.ForumComment;

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
        public DateTime? UpdatedAt { get; set; }
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; }

        
        public bool HasUpvoted { get; set; }
        public bool CanEditDelete { get; set; }
        public bool CanMarkAsSolved { get; set; }
        public bool IsPinned { get; set; }
        public string UserName { get; set; }
        public string? UserProfilePicture { get; set; }
        public List<ForumCommentDto> Comments { get; set; } = new();
    }
}
