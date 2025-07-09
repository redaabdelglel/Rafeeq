using Rafeeq.Models;
using Rafeeq.Repositories.RepositoryBase;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Rafeeq.Repositories.Forum
{
    public class ForumPostRepository : RepositoryBase<ForumPost>, IForumPostRepository
    {
        public ForumPostRepository(RafeeqContext context) : base(context) { }

        public IQueryable<ForumPost> GetPostsQuery(
            int? categoryId = null,
            string? searchQuery = null,
            string? sortBy = "recent",
            bool? isSolved = null)
        {
            IQueryable<ForumPost> query = GetQuery().Where(p => p.IsDeleted == false);

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                var lowerCaseQuery = searchQuery.ToLower();
                query = query.Where(p =>
                    p.Title.ToLower().Contains(lowerCaseQuery) ||
                    p.Content.ToLower().Contains(lowerCaseQuery)
                );
            }

            if (isSolved.HasValue)
            {
                query = query.Where(p => p.IsSolved == isSolved.Value);
            }

            query = query.Include(p => p.User).Include(p => p.Category); // Include User and Category

            switch (sortBy?.ToLower())
            {
                case "upvotes":
                    query = query.OrderByDescending(p => p.Upvotes);
                    break;
                case "recent":
                default:
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            return query;
        }

        public async Task<ForumPost?> GetPostByIdWithDetailsAsync(int postId)
        {
            return await GetQuery()
                .Where(p => p.PostId == postId && p.IsDeleted == false)
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Comments.Where(c => c.IsDeleted == false)).ThenInclude(c => c.User)
                .FirstOrDefaultAsync();
        }

        public IQueryable<ForumPost> GetPostsByUserQuery(int userId)
        {
            return GetQuery()
                .Where(p => p.UserId == userId && p.IsDeleted == false)
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt);
        }

        public async Task<ForumPostUpvote?> GetUserUpvoteForPostAsync(int postId, int userId)
        {
            return await Context.Set<ForumPostUpvote>()
                .FirstOrDefaultAsync(up => up.PostId == postId && up.UserId == userId);
        }
    }
}