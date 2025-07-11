
using Rafeeq.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rafeeq.Repositories.Forum
{
    public interface IForumPostMgmtRepository
    {
        Task<List<ForumPost>> GetAllAsync(int? categoryId = null, string? search = null, string? sortBy = "recent", bool? isSolved = null);
        Task<ForumPost?> GetByIdWithDetailsAsync(int postId);
        Task<List<ForumPost>> GetByUserIdAsync(int userId);
        Task AddAsync(ForumPost post);
        void Update(ForumPost post);
        void Delete(ForumPost post);
        Task SaveAsync();
        Task<ForumPostUpvote?> GetUserUpvoteAsync(int postId, int userId);
        Task AddUpvoteAsync(ForumPostUpvote upvote);
        void RemoveUpvote(ForumPostUpvote upvote);

        Task AddReportAsync(ForumPostReport report);
        Task<List<ForumPostReport>> GetAllReportsAsync();
        Task<ForumPostReport?> GetReportByIdAsync(int reportId);
        void UpdateReport(ForumPostReport report);

    }
}
