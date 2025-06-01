using Microsoft.EntityFrameworkCore;
using Rafeeq.Models;

namespace Rafeeq.Repositories.Bookings
{
    public class BookingRepository
    {
        private readonly RafeeqContext _context;

        public BookingRepository(RafeeqContext context)
        {
            _context = context;
        }

        // get all bookings
        public async Task<IEnumerable<Booking>> GetAllAsync()
        {
            return await _context.Bookings.ToListAsync();
        }

        // get total revenue
       
        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Bookings
                .Where(b => b.Status == "Completed")
                .SumAsync(b => b.TotalAmount ?? 0); 
        }


    }
}
