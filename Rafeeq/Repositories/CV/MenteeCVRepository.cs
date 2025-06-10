using Rafeeq.Models;
using Microsoft.EntityFrameworkCore;

namespace Rafeeq.Repositories.CV
{
    public interface ICVRepository
    {
        Task<IEnumerable<MenteeCV>> GetMenteeCVsAsync(int userId);
        Task<MenteeCV> UploadCVAsync(MenteeCV cv);
        Task<bool> DeleteCVAsync(int cvId);
        Task<IEnumerable<CVComment>> GetCVCommentsAsync(int cvId);
        Task<CVComment> AddCVCommentAsync(CVComment comment);
        Task<MenteeCV> GetCurrentCVAsync(int userId);
        Task<MenteeCV> GetCVByIdAsync(int cvId); // Added new method
    }

    public class MenteeCVRepository : ICVRepository
    {
        private readonly RafeeqContext _context;
        private readonly IWebHostEnvironment _environment;

        public MenteeCVRepository(RafeeqContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IEnumerable<MenteeCV>> GetMenteeCVsAsync(int userId)
        {
            return await _context.MenteeCVs
                .Where(cv => cv.UserId == userId && cv.IsActive)
                .OrderByDescending(cv => cv.UploadDate)
                .ToListAsync();
        }

        public async Task<MenteeCV> GetCVByIdAsync(int cvId)
        {
            return await _context.MenteeCVs
                .FirstOrDefaultAsync(cv => cv.CVId == cvId);
        }

        public async Task<MenteeCV> UploadCVAsync(MenteeCV cv)
        {
            // Deactivate all previous CVs
            var previousCVs = await _context.MenteeCVs
                .Where(c => c.UserId == cv.UserId && c.IsActive)
                .ToListAsync();

            //foreach (var prevCV in previousCVs)
            //{
            //    prevCV.IsActive = false;
            //}

            // Add new CV
            cv.UploadDate = DateTime.Now;
            cv.IsActive = true;
            _context.MenteeCVs.Add(cv);
            await _context.SaveChangesAsync();
            return cv;
        }

        public async Task<bool> DeleteCVAsync(int cvId)
        {
            var cv = await _context.MenteeCVs.FindAsync(cvId);
            if (cv == null) return false;

            cv.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CVComment>> GetCVCommentsAsync(int cvId)
        {
            return await _context.CVComments
                .Include(c => c.Mentor)
                .Where(c => c.CVId == cvId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<CVComment> AddCVCommentAsync(CVComment comment)
        {
            comment.CreatedAt = DateTime.Now;
            _context.CVComments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<MenteeCV> GetCurrentCVAsync(int userId)
        {
            return await _context.MenteeCVs
                .FirstOrDefaultAsync(cv => cv.UserId == userId && cv.IsActive);
        }
    }
}