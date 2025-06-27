using Microsoft.EntityFrameworkCore;
using Rafeeq.DTOs.Reviews;
using Rafeeq.Models;

namespace Rafeeq.Repositories.Reviews
{
    public class ReviewRepository
    {
        private readonly RafeeqContext _context;

        public ReviewRepository(RafeeqContext context)
        {
            _context = context;
        }
       
        public async Task<IEnumerable<ReviewDateDto>> GetReviewsByMentorIdAsync(int mentorId)
        {
            if (mentorId <= 0)
            {
                throw new ArgumentException("Invalid mentor ID.", nameof(mentorId));
            }

            return await _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.ReviewedUser)
                .Where(r => r.ReviewedUser.Role.RoleName == "Mentor")
                .Select(r => new ReviewDateDto
                {
                    ReviewId = r.ReviewId,
                    ReviewerId = r.ReviewerId,
                    ReviewedUserId = r.ReviewedUserId,
                    CreatedAt = r.CreatedAt,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    BookingId = r.BookingId ?? 0
                })
                .ToListAsync();
        }


        public async Task<IEnumerable<Review>> GetReviewsByMenteeIdAsync(int menteeId)
        {
            if (menteeId <= 0)
            {
                throw new ArgumentException("Invalid mentee ID.", nameof(menteeId));
            }
            return await _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.ReviewedUser)
                .Where(r => r.ReviewedUser.Role.RoleName == "Mentee")
                .ToListAsync();
        }




        //get all reviews
        public async Task<IEnumerable<ReviewDto>> GetAllReviewsAsync()
        {
            return await _context.Reviews
                .Include(s => s.ReviewedUser)
                .Include(d => d.Reviewer)
                .Select(r => new ReviewDto
                {
                    ReviewId = r.ReviewId,
                    ReviewerId = r.ReviewerId,
                    ReviewedUserId = r.ReviewedUserId,
                    CreatedAt = r.CreatedAt,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewedUserName = r.ReviewedUser.FullName,
                    ReviewerName = r.Reviewer.FullName

                    
                })
                .ToListAsync();
        }


        // Get review by ID
        public async Task<ReviewDto> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid review ID.", nameof(id));
            }

            var review = await _context.Reviews
                .AsNoTracking()
                .Where(r => r.ReviewId == id)
                .Select(r => new ReviewDto
                {
                    ReviewId = r.ReviewId,
                    ReviewerId = r.ReviewerId,
                    ReviewedUserId = r.ReviewedUserId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .FirstOrDefaultAsync();

            return review;
        }
        // delete review by ID
        public async Task<bool> DeleteAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return false;
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
