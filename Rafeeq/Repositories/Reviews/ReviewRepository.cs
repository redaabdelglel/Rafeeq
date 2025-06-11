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
        public async Task<IEnumerable<Review>> GetReviewsByMentorIdAsync(int mentorId)
        {
            if (mentorId <= 0)
            {
                throw new ArgumentException("Invalid mentor ID.", nameof(mentorId));
            }
            return await _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.ReviewedUser)
                .Where(r => r.Reviewer.Role.RoleName == "Mentor")
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
                .Where(r => r.Reviewer.Role.RoleName == "Mentee")
                .ToListAsync();
        }
        // Add a new review
        public async Task<Review> AddAsync(CreateReviewDto review)
        {
            if (review == null)
            {
                throw new ArgumentNullException(nameof(review), "Review data is required.");
            }
            var newReview = new Review
            {
                ReviewerId = review.ReviewerId,
                ReviewedUserId = review.ReviewedUserId,
                BookingId = review.BookingId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Reviews.Add(newReview);
            await _context.SaveChangesAsync();
            return newReview;
        }

        // Get all reviews
        public async Task<IEnumerable<Review>> GetAllAsync()
        {
            return await _context.Reviews.ToListAsync();
        }
        // Get review by ID
        public async Task<Review> GetByIdAsync(int id)
        {
            return await _context.Reviews.FindAsync(id);
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
