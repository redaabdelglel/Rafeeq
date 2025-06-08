using Microsoft.EntityFrameworkCore;
using Rafeeq.Models;
using System.Threading.Tasks;

namespace Rafeeq.Repositories.CV
{
    public class CVCommentRepository
    {
        private readonly RafeeqContext _context;

        public CVCommentRepository(RafeeqContext context)
        {
            _context = context;
        }

        // Add a new CV comment
        public async Task<CVComment> AddAsync(CVComment comment)
        {
            comment.CreatedAt = DateTime.UtcNow;
            await _context.CVComments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        // Get a CV comment by ID
        public async Task<CVComment> GetByIdAsync(int id)
        {
            return await _context.CVComments
                .Include(c => c.Mentor)
                .Include(c => c.CV)
                .FirstOrDefaultAsync(c => c.CommentId == id);
        }

        // Delete a CV comment
        public async Task<bool> DeleteAsync(int id)
        {
            var comment = await _context.CVComments.FindAsync(id);
            if (comment == null)
            {
                return false;
            }

            _context.CVComments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        // Get all comments for a CV
        public async Task<IEnumerable<CVComment>> GetByCVIdAsync(int cvId)
        {
            return await _context.CVComments
                .Include(c => c.Mentor)
                .Where(c => c.CVId == cvId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        // Check if a user is the author of a comment
        public async Task<bool> IsMentorAuthorAsync(int commentId, int mentorId)
        {
            return await _context.CVComments
                .AnyAsync(c => c.CommentId == commentId && c.MentorId == mentorId);
        }

        // Check if a CV exists
        public async Task<bool> DoesCVExistAsync(int cvId)
        {
            return await _context.MenteeCVs.AnyAsync(cv => cv.CVId == cvId);
        }
    }
}
