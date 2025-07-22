
using Rafeeq.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rafeeq.Repositories.Forum
{
    public class ForumPostMgmtRepository : IForumPostMgmtRepository
    {
        private readonly RafeeqContext _context;
        public ForumPostMgmtRepository(RafeeqContext context) { _context = context; }

        public async Task<List<ForumPost>> GetAllAsync(int? categoryId = null, string? search = null, string? sortBy = "recent", bool? isSolved = null)
        {
            var query = _context.ForumPosts
                .Include(p => p.User)
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted);

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (!string.IsNullOrEmpty(search))
            {
                var lower = search.ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(lower) || p.Content.ToLower().Contains(lower));
            }

            if (isSolved.HasValue)
                query = query.Where(p => p.IsSolved == isSolved.Value);

            query = sortBy == "upvotes"
                ? query.OrderByDescending(p => p.Upvotes)
                : query.OrderByDescending(p => p.CreatedAt);

            return await query.ToListAsync();
        }

        public async Task<ForumPost?> GetByIdWithDetailsAsync(int postId)
        {
            return await _context.ForumPosts
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Comments.Where(c => !c.IsDeleted)).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.PostId == postId && !p.IsDeleted);
        }

        public async Task<List<ForumPost>> GetByUserIdAsync(int userId)
        {
            return await _context.ForumPosts
                .Include(p => p.Category)
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(ForumPost post)
        {
            await _context.ForumPosts.AddAsync(post);
        }

        public void Update(ForumPost post)
        {
            _context.ForumPosts.Update(post);
        }

        public void Delete(ForumPost post)
        {
            _context.ForumPosts.Remove(post);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<ForumPostUpvote?> GetUserUpvoteAsync(int postId, int userId)
        {
            return await _context.ForumPostUpvotes.FirstOrDefaultAsync(u => u.PostId == postId && u.UserId == userId);
        }

        public async Task AddUpvoteAsync(ForumPostUpvote upvote)
        {
            await _context.ForumPostUpvotes.AddAsync(upvote);
        }

        public void RemoveUpvote(ForumPostUpvote upvote)
        {
            _context.ForumPostUpvotes.Remove(upvote);
        }

        public async Task AddReportAsync(ForumPostReport report)
        {
            await _context.ForumPostReports.AddAsync(report);
        }

        public async Task<List<ForumPostReport>> GetAllReportsAsync()
        {
            return await _context.ForumPostReports
                .Include(r => r.Post)
                .Include(r => r.ReportedByUser)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<ForumPostReport?> GetReportByIdAsync(int reportId)
        {
            return await _context.ForumPostReports
                .Include(r => r.Post)
                .Include(r => r.ReportedByUser)
                .FirstOrDefaultAsync(r => r.ReportId == reportId);
        }

        public void UpdateReport(ForumPostReport report)
        {
            _context.ForumPostReports.Update(report);
        }

    }
}
