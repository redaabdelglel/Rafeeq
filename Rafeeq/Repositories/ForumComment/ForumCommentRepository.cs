using Rafeeq.Models;
using Rafeeq.Repositories.RepositoryBase;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Rafeeq.Repositories.Forum
{
    public class ForumCommentRepository : RepositoryBase<ForumComment>, IForumCommentRepository
    {
        public ForumCommentRepository(RafeeqContext context) : base(context) { }

        public IQueryable<ForumComment> GetCommentsByPostQuery(int postId)
        {
            return GetQuery()
                .Where(c => c.PostId == postId && c.IsDeleted == false) 
                .Include(c => c.User)
                .OrderBy(c => c.CreatedAt);
        }

        public async Task<ForumComment?> GetCommentByIdWithUserAsync(int commentId)
        {
            return await Context.ForumComments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CommentId == commentId && !c.IsDeleted);
        }

    }
}