using Rafeeq.DTOs.Forum;
using Rafeeq.Models;
using Rafeeq.Repositories.Forum;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rafeeq.Services.Forum
{
    public class ForumPostMgmtService : IForumPostMgmtService
    {
        private readonly IForumPostMgmtRepository _repo;
        private readonly IMapper _mapper;

        public ForumPostMgmtService(IForumPostMgmtRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<ForumPostDto>> GetAllAsync(int? categoryId = null, string? search = null, string? sortBy = "recent", bool? isSolved = null)
        {
            var posts = await _repo.GetAllAsync(categoryId, search, sortBy, isSolved);

            // Ensure pinned posts are always on top, then sort by created date or upvotes
            var orderedPosts = posts.OrderByDescending(p => p.IsPinned);

            if (sortBy == "upvotes")
                orderedPosts = orderedPosts.ThenByDescending(p => p.Upvotes);
            else
                orderedPosts = orderedPosts.ThenByDescending(p => p.CreatedAt);

            return orderedPosts.Select(_mapper.Map<ForumPostDto>).ToList();

        }


        public async Task<ForumPostDto?> GetByIdAsync(int postId)
        {
            var post = await _repo.GetByIdWithDetailsAsync(postId);
            return post == null ? null : _mapper.Map<ForumPostDto>(post);
        }

        public async Task<List<ForumPostDto>> GetByUserIdAsync(int userId)
        {
            var posts = await _repo.GetByUserIdAsync(userId);
            return posts.Select(_mapper.Map<ForumPostDto>).ToList();
        }

        public async Task<ForumPostDto> CreateAsync(CreateForumPostDto dto, int userId)
        {
            var post = _mapper.Map<ForumPost>(dto);
            post.UserId = userId;
            post.CreatedAt = DateTime.UtcNow;
            post.IsDeleted = false;
            post.Upvotes = 0;
            post.IsSolved = false;
            await _repo.AddAsync(post);
            await _repo.SaveAsync();
            return _mapper.Map<ForumPostDto>(post);
        }

        public async Task<bool> UpdateAsync(int postId, UpdateForumPostDto dto, int userId)
        {
            var post = await _repo.GetByIdWithDetailsAsync(postId);
            if (post == null || post.UserId != userId || post.IsDeleted) return false;
            post.Title = dto.Title;
            post.Content = dto.Content;
            post.CategoryId = dto.CategoryId;
            post.UpdatedAt = DateTime.UtcNow;
            _repo.Update(post);
            await _repo.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int postId, int userId)
        {
            var post = await _repo.GetByIdWithDetailsAsync(postId);
            if (post == null || post.UserId != userId || post.IsDeleted) return false;
            post.IsDeleted = true;
            _repo.Update(post);
            await _repo.SaveAsync();
            return true;
        }

        public async Task<bool> UpvoteAsync(int postId, int userId)
        {
            var post = await _repo.GetByIdWithDetailsAsync(postId);
            if (post == null || post.IsDeleted) return false;
            var upvote = await _repo.GetUserUpvoteAsync(postId, userId);
            if (upvote != null) return false; // already upvoted
            var newUpvote = new ForumPostUpvote { PostId = postId, UserId = userId, CreatedAt = DateTime.UtcNow };
            await _repo.AddUpvoteAsync(newUpvote);
            post.Upvotes += 1;
            _repo.Update(post);
            await _repo.SaveAsync();
            return true;
        }

        public async Task<bool> RemoveUpvoteAsync(int postId, int userId)
        {
            var post = await _repo.GetByIdWithDetailsAsync(postId);
            if (post == null || post.IsDeleted) return false;
            var upvote = await _repo.GetUserUpvoteAsync(postId, userId);
            if (upvote == null) return false;
            _repo.RemoveUpvote(upvote);
            post.Upvotes = Math.Max(0, post.Upvotes - 1);
            _repo.Update(post);
            await _repo.SaveAsync();
            return true;
        }

        public async Task<bool> MarkAsSolvedAsync(int postId, int userId)
        {
            var post = await _repo.GetByIdWithDetailsAsync(postId);
            if (post == null || post.UserId != userId || post.IsDeleted) return false;
            post.IsSolved = true;
            _repo.Update(post);
            await _repo.SaveAsync();
            return true;
        }

        public async Task<bool> ReportPostAsync(int postId, int userId, string reason)
        {
            // Check if post exists and is not deleted
            var post = await _repo.GetByIdWithDetailsAsync(postId);
            if (post == null || post.IsDeleted)
                return false;

            // Prevent duplicate reports by the same user for the same post
            var existingReports = await _repo.GetAllReportsAsync();
            if (existingReports.Any(r => r.PostId == postId && r.ReportedByUserId == userId))
                return false;

            var report = new ForumPostReport
            {
                PostId = postId,
                ReportedByUserId = userId,
                Reason = reason,
                CreatedAt = DateTime.UtcNow,
                Status = "Pending"
            };

            await _repo.AddReportAsync(report);
            await _repo.SaveAsync();
            return true;
        }

        public async Task<List<ForumPostReportDto>> GetAllReportsAsync()
        {
            var reports = await _repo.GetAllReportsAsync();
            return reports.Select(r => new ForumPostReportDto
            {
                ReportId = r.ReportId,
                PostId = r.PostId,
                ReportedByUserId = r.ReportedByUserId,
                Reason = r.Reason,
                CreatedAt = r.CreatedAt,
                Status = r.Status,
                AdminNote = r.AdminNote,
                PostTitle = r.Post?.Title ?? "",
                PostOwnerName = r.Post?.User?.FullName,
                ReportedByUserName = r.ReportedByUser?.FullName ?? ""
            }).ToList();
        }


        public async Task<bool> TakeActionOnReportAsync(int reportId, string action, string? adminNote)
        {
            var report = await _repo.GetReportByIdAsync(reportId);
            if (report == null || report.Status != "Pending")
                return false;

            // Only allow "delete" or "ignore"
            if (action.Equals("delete", StringComparison.OrdinalIgnoreCase))
            {
                // Soft delete the post
                var post = await _repo.GetByIdWithDetailsAsync(report.PostId);
                if (post != null && !post.IsDeleted)
                {
                    post.IsDeleted = true;
                    _repo.Update(post);
                }
                report.Status = "Resolved";
                report.AdminNote = adminNote ?? "Post deleted by admin.";
            }
            else if (action.Equals("ignore", StringComparison.OrdinalIgnoreCase))
            {
                report.Status = "Ignored";
                report.AdminNote = adminNote ?? "Report ignored by admin.";
            }
            else
            {
                return false;
            }

            _repo.UpdateReport(report);
            await _repo.SaveAsync();
            return true;
        }

        public async Task AddReportAsync(ForumPostReport report)
        {
            await _repo.AddReportAsync(report);
            await _repo.SaveAsync();
        }

        public async Task<ForumPostReport?> GetReportByIdAsync(int reportId)
        {
            return await _repo.GetReportByIdAsync(reportId);
        }

        public void UpdateReport(ForumPostReport report)
        {
            _repo.UpdateReport(report);
        }

        public async Task<bool> PinPostAsync(int postId)
        {
            var post = await _repo.GetByIdWithDetailsAsync(postId);
            if (post == null || post.IsDeleted) return false;
            post.IsPinned = true;
            _repo.Update(post);
            await _repo.SaveAsync();
            return true;
        }

        public async Task<bool> UnpinPostAsync(int postId)
        {
            var post = await _repo.GetByIdWithDetailsAsync(postId);
            if (post == null || post.IsDeleted) return false;
            post.IsPinned = false;
            _repo.Update(post);
            await _repo.SaveAsync();
            return true;
        }

    }
}
