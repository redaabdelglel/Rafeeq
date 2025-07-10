using Rafeeq.DTOs.Forum;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rafeeq.Services.Forum
{
    public interface IForumPostMgmtService
    {
        Task<List<ForumPostDto>> GetAllAsync(int? categoryId = null, string? search = null, string? sortBy = "recent", bool? isSolved = null);
        Task<ForumPostDto?> GetByIdAsync(int postId);
        Task<List<ForumPostDto>> GetByUserIdAsync(int userId);
        Task<ForumPostDto> CreateAsync(CreateForumPostDto dto, int userId);
        Task<bool> UpdateAsync(int postId, UpdateForumPostDto dto, int userId);
        Task<bool> DeleteAsync(int postId, int userId);
        Task<bool> UpvoteAsync(int postId, int userId);
        Task<bool> RemoveUpvoteAsync(int postId, int userId);
        Task<bool> MarkAsSolvedAsync(int postId, int userId);
    }
}
