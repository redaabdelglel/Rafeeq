using Microsoft.EntityFrameworkCore;
using Rafeeq.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rafeeq.Repositories.Reviews
{
    public interface IMenteeReviewsRepository
    {
        Task<IEnumerable<Review>> GetMentorReviewsAsync(int mentorId);
        Task<IEnumerable<Review>> GetMenteeReviewsAsync(int menteeId);
        Task<Review> GetReviewByIdAsync(int reviewId);
        Task<Review> CreateReviewAsync(Review review);
        Task<Review> UpdateReviewAsync(Review review);
        Task<bool> DeleteReviewAsync(int reviewId);
        Task<bool> CanMenteeReviewMentorAsync(int menteeId, int mentorId);
        Task<Review> GetReviewForBookingAsync(int bookingId);
    }

    public class MenteeReviewsRepository : IMenteeReviewsRepository
    {
        private readonly RafeeqContext _context;

        public MenteeReviewsRepository(RafeeqContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Review>> GetMentorReviewsAsync(int mentorId)
        {
            return await _context.Reviews
                .Include(r => r.Reviewer)
                .Where(r => r.ReviewedUserId == mentorId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetMenteeReviewsAsync(int menteeId)
        {
            return await _context.Reviews
                .Include(r => r.ReviewedUser)
                .Where(r => r.ReviewerId == menteeId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review> GetReviewByIdAsync(int reviewId)
        {
            return await _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.ReviewedUser)
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId);
        }

        public async Task<Review> CreateReviewAsync(Review review)
        {
            review.CreatedAt = DateTime.Now;
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<Review> UpdateReviewAsync(Review review)
        {
            review.UpdatedAt = DateTime.Now;
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<bool> DeleteReviewAsync(int reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
                return false;

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CanMenteeReviewMentorAsync(int menteeId, int mentorId)
        {
            return await _context.Bookings
                .AnyAsync(b => b.MenteeId == menteeId &&
                             b.MentorId == mentorId &&
                             b.Status == "Completed" &&
                             b.EndDateTime < DateTime.Now);
        }

        public async Task<Review> GetReviewForBookingAsync(int bookingId)
        {
            return await _context.Reviews
                .FirstOrDefaultAsync(r => r.BookingId == bookingId);
        }
    }
}