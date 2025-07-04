using Microsoft.EntityFrameworkCore;
using Rafeeq.DTOs.Payments;
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
        public async Task<IEnumerable<PaymentDto>> GetAllAsync()
        {
            return await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Mentor)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Mentee)
                .Select(p => new PaymentDto
                {
                    PaymentId = p.PaymentId,
                    BookingId = p.BookingId ?? 0,
                    AmountPaid = p.AmountPaid ?? 0,
                    PaymentMethod = p.PaymentMethod,
                    TransactionId = p.TransactionId,
                    PaymentDate = p.PaymentDate ?? DateTime.MinValue,
                    MentorName = p.Booking.Mentor.FullName,
                    MenteeName = p.Booking.Mentee.FullName,

                   
                    SessionType = p.Booking.SessionType,
                    SessionDateTime = p.Booking.StartDateTime ?? DateTime.MinValue,
                    Commission = p.Booking.Commission ?? 0,
                    MentorAmount = (p.AmountPaid ?? 0) - (p.Booking.Commission ?? 0)
                })
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

            return payments.Sum(p => (p.AmountPaid ?? 0) - (p.Booking.Commission ?? 0));
        }

        // Get earnings for specific months
        public async Task<decimal> GetMonthlyEarningsAsync(int mentorId, int year, int month)
        {
            var payments = await _context.Payments
                .Include(p => p.Booking)
                .Where(p => p.Booking.MentorId == mentorId &&
                           p.PaymentDate.HasValue &&
                           p.PaymentDate.Value.Year == year &&
                           p.PaymentDate.Value.Month == month)
                .ToListAsync();

            return payments.Sum(p => (p.AmountPaid ?? 0) - (p.Booking.Commission ?? 0));
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
                           p.PaymentDate.HasValue &&
                           p.PaymentDate.Value >= startDate &&
                           p.PaymentDate.Value <= endDate)
                .ToListAsync();

            return payments
                .Where(p => p.PaymentDate.HasValue)
                .GroupBy(p => new { Year = p.PaymentDate.Value.Year, Month = p.PaymentDate.Value.Month })
                .Select(g => (
                    Year: g.Key.Year,
                    Month: g.Key.Month,
                    Amount: g.Sum(p => (p.AmountPaid ?? 0) - (p.Booking.Commission ?? 0))
                ))
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();
        }
    }
}
