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

        public async Task<IEnumerable<ReviewDateDto>> GetReviewsWrittenByUserAsync(int userId)
        {
            return await _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.ReviewedUser)
                .Where(r => r.ReviewerId == userId)
                .Select(r => new ReviewDateDto
                {
                    ReviewId = r.ReviewId,
                    ReviewerId = r.ReviewerId,
                    ReviewerName = r.Reviewer.FullName,
                    ReviewerRole = r.Reviewer.Role.RoleName,
                    ReviewedUserId = r.ReviewedUserId,
                    ReviewedUserName = r.ReviewedUser.FullName,
                    ReviewedUserRole = r.ReviewedUser.Role.RoleName,
                    BookingId = r.BookingId ?? 0,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ReviewDateDto>> GetReviewsAboutUserAsync(int userId)
        {
            return await _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.ReviewedUser)
                .Where(r => r.ReviewedUserId == userId)
                .Select(r => new ReviewDateDto
                {
                    ReviewId = r.ReviewId,
                    ReviewerId = r.ReviewerId,
                    ReviewerName = r.Reviewer.FullName,
                    ReviewerRole = r.Reviewer.Role.RoleName,
                    ReviewedUserId = r.ReviewedUserId,
                    ReviewedUserName = r.ReviewedUser.FullName,
                    ReviewedUserRole = r.ReviewedUser.Role.RoleName,
                    BookingId = r.BookingId ?? 0,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
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

        public async Task CreateReview(Review review)
        {
            if (review == null)
            {
                throw new ArgumentNullException(nameof(review), "Review cannot be null.");
            }
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
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

        public async Task<Review> GetByBookingIdAsync(int bookingId)
        {
            return await _context.Reviews
                .FirstOrDefaultAsync(r => r.BookingId == bookingId);
        }


    }
}