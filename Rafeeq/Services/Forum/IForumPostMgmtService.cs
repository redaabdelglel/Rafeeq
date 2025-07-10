using Rafeeq.DTOs.Forum;
using Rafeeq.Models;
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

        
        Task AddReportAsync(ForumPostReport report);
        
        Task<ForumPostReport?> GetReportByIdAsync(int reportId);
        void UpdateReport(ForumPostReport report);

        Task<bool> ReportPostAsync(int postId, int userId, string reason);
        Task<List<ForumPostReportDto>> GetAllReportsAsync();

        Task<bool> TakeActionOnReportAsync(int reportId, string action, string? adminNote);

        Task<bool> PinPostAsync(int postId);
        Task<bool> UnpinPostAsync(int postId);

    }
}
