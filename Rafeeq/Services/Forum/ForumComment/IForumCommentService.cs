using Rafeeq.DTOs.ForumComment;

namespace Rafeeq.Services.Forum.ForumComment
{
    public interface IForumCommentService
    {
        Task<ForumCommentDto?> CreateForumCommentAsync(int postId, int userId, CreateForumCommentDto dto);
        Task<bool> UpdateForumCommentAsync(int commentId, int currentUserId, UpdateForumCommentDto dto);
        Task<bool> DeleteForumCommentAsync(int commentId, int currentUserId);
    }
}
