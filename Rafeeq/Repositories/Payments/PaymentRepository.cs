using Microsoft.EntityFrameworkCore;
using Rafeeq.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Mentor)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Mentee)
                .ToListAsync();
        }

        // Get payment by ID
        public async Task<Payment> GetByIdAsync(int id)
        {
            return await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Mentor)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Mentee)
                .FirstOrDefaultAsync(p => p.PaymentId == id);
        }

        // Get payments for a booking
        public async Task<Payment> GetByBookingIdAsync(int bookingId)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.BookingId == bookingId);
        }

        // Add new payment
        public async Task<Payment> AddAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        // Get payments for a user (either mentor or mentee)
        public async Task<IEnumerable<Payment>> GetPaymentHistoryAsync(int userId)
        {
            return await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Mentor)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Mentee)
                .Where(p => p.Booking.MentorId == userId || p.Booking.MenteeId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        // Get earnings for a mentor
        public async Task<decimal> GetTotalEarningsAsync(int mentorId)
        {
            // Calculate mentor earnings after platform commission
            var payments = await _context.Payments
                .Include(p => p.Booking)
                .Where(p => p.Booking.MentorId == mentorId)
                .ToListAsync();

            return payments.Sum(p => p.AmountPaid - p.Booking.Commission);
        }

        // Get earnings for specific months
        public async Task<decimal> GetMonthlyEarningsAsync(int mentorId, int year, int month)
        {
            var payments = await _context.Payments
                .Include(p => p.Booking)
                .Where(p => p.Booking.MentorId == mentorId &&
                           p.PaymentDate.Year == year &&
                           p.PaymentDate.Month == month)
                .ToListAsync();

            return payments.Sum(p => p.AmountPaid - p.Booking.Commission);
        }

        // Get payment counts by status
        public async Task<int> GetCompletedSessionsCountAsync(int mentorId)
        {
            return await _context.Bookings
                .CountAsync(b => b.MentorId == mentorId &&
                                b.Status == "Completed" &&
                                b.PaymentStatus == "Paid");
        }

        // Get upcoming sessions count
        public async Task<int> GetUpcomingSessionsCountAsync(int mentorId)
        {
            return await _context.Bookings
                .CountAsync(b => b.MentorId == mentorId &&
                                (b.Status == "Confirmed" || b.Status == "Pending") &&
                                b.StartDateTime > DateTime.UtcNow);
        }

        // Get monthly earnings breakdown for the past year
        public async Task<List<(int Year, int Month, decimal Amount)>> GetMonthlyEarningsBreakdownAsync(int mentorId)
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddYears(-1);

            var payments = await _context.Payments
                .Include(p => p.Booking)
                .Where(p => p.Booking.MentorId == mentorId &&
                           p.PaymentDate >= startDate &&
                           p.PaymentDate <= endDate)
                .ToListAsync();

            return payments
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .Select(g => (
                    Year: g.Key.Year,
                    Month: g.Key.Month,
                    Amount: g.Sum(p => p.AmountPaid - p.Booking.Commission)
                ))
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();
        }
    }
}
