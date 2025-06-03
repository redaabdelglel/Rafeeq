using Microsoft.EntityFrameworkCore;
using Rafeeq.DTOs.Bookings;
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
        public async Task<IEnumerable<BookingDto>> GetAllAsync()
        {
            return await _context.Bookings
       .Include(b => b.Mentor)
       .Include(b => b.Mentee)
       .Where(b => b.IsDeleted == false)
       .Select(b => new BookingDto
       {
           BookingId = b.BookingId,
           SessionType = b.SessionType,
           StartDateTime = b.StartDateTime,       
           EndDateTime = b.EndDateTime,
           Status = b.Status,
           GoogleMeetLink = b.GoogleMeetLink,
           PaymentStatus = b.PaymentStatus,
           TotalAmount = b.TotalAmount ?? 0,                      
           Commission = b.Commission ?? 0,                            
           MentorName = b.Mentor.FullName,
           MenteeName = b.Mentee.FullName
       })
       .ToListAsync();

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
