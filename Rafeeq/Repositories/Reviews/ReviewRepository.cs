using Microsoft.EntityFrameworkCore;
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
