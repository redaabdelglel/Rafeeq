using Rafeeq.Models;
using Rafeeq.Repositories.RepositoryBase;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rafeeq.Repositories.Forum
{
    public interface IForumPostRepository : IRepositoryBase<ForumPost>
    {
        IQueryable<ForumPost> GetPostsQuery(
            int? categoryId = null,
            string? searchQuery = null,
            string? sortBy = "recent",
            bool? isSolved = null);

        Task<ForumPost?> GetPostByIdWithDetailsAsync(int postId);
        IQueryable<ForumPost> GetPostsByUserQuery(int userId);
        Task<ForumPostUpvote?> GetUserUpvoteForPostAsync(int postId, int userId);
    }
}