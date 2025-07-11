
using Rafeeq.Repositories.RepositoryBase;

namespace Rafeeq.Repositories.Forum 
{

    public interface IForumCommentRepository : IRepositoryBase<Rafeeq.Models.ForumComment>
    {
        IQueryable<Rafeeq.Models.ForumComment> GetCommentsByPostQuery(int postId);
        Task<Rafeeq.Models.ForumComment?> GetCommentByIdWithUserAsync(int commentId);
    }
}