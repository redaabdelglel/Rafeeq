using Microsoft.EntityFrameworkCore;
using Rafeeq.DTOs.Payments;
using Rafeeq.Models;

namespace Rafeeq.Repositories.Payments
{
    public class PaymentRepository
    {
        private readonly RafeeqContext _context;

        public PaymentRepository(RafeeqContext context)
        {
            _context = context;
        }

        // Get all payments
        public async Task<IEnumerable<PaymentDto>> GetAllAsync()
        {
            return await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Mentee)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Mentor)
                .Select(p => new PaymentDto
                {
                    PaymentId = p.PaymentId,
                    BookingId = p.BookingId ?? 0, 
                    AmountPaid = p.AmountPaid ?? 0, 
                    TransactionId = p.TransactionId,
                    PaymentDate = p.PaymentDate ?? DateTime.MinValue, 
                    MenteeFullName = p.Booking.Mentee.FullName,
                    MentorFullName = p.Booking.Mentor.FullName
                })
                .ToListAsync();
        }
    }
}
