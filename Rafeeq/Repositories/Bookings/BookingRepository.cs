using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rafeeq.Models;

namespace Rafeeq.Repositories.Bookings
{
        public interface IBookingRepository
        {
            Task<IEnumerable<Booking>> GetMenteeBookingsAsync(int menteeId);
            Task<IEnumerable<Booking>> GetUpcomingBookingsAsync(int userId);
            Task<IEnumerable<Booking>> GetCompletedBookingsAsync(int userId);
            Task<Booking> GetBookingDetailsAsync(int id);
            Task<Booking> CreateBookingAsync(Booking booking);
            Task<string> GetGoogleMeetLinkAsync(int bookingId);
            Task<Booking> UpdateBookingAsync(Booking booking);
            Task<bool> DeleteBookingAsync(int id);
            Task<bool> SoftDeleteBookingAsync(int id);


    }

    public class BookingRepository : IBookingRepository
        {
            private readonly RafeeqContext _context;
            private readonly ILogger<BookingRepository> _logger;  // Add logging

        public BookingRepository(RafeeqContext context)
        {
            _context = context;

        }
        public BookingRepository(RafeeqContext context, ILogger<BookingRepository> logger)
            {
                _context = context;
                _logger = logger;

        }

        public async Task<IEnumerable<Booking>> GetMenteeBookingsAsync(int menteeId)
            {
                return await _context.Bookings
                    .Include(b => b.Mentor)
                    .Where(b => b.MenteeId == menteeId && !b.IsDeleted.Value)
                    .OrderByDescending(b => b.StartDateTime)
                    .ToListAsync();
            }

            public async Task<IEnumerable<Booking>> GetUpcomingBookingsAsync(int userId)
            {
                return await _context.Bookings
                    .Include(b => b.Mentor)
                    .Include(b => b.Mentee)
                    .Where(b => (b.MenteeId == userId || b.MentorId == userId) &&
                                b.StartDateTime > DateTime.Now &&
                                !b.IsDeleted.Value)
                    .OrderBy(b => b.StartDateTime)
                    .ToListAsync();
            }

            public async Task<IEnumerable<Booking>> GetCompletedBookingsAsync(int userId)
            {
                return await _context.Bookings
                    .Include(b => b.Mentor)
                    .Include(b => b.Mentee)
                    .Where(b => (b.MenteeId == userId || b.MentorId == userId) &&
                                b.EndDateTime < DateTime.Now &&
                                !b.IsDeleted.Value)
                    .OrderByDescending(b => b.StartDateTime)
                    .ToListAsync();
            }

        public async Task<Booking> GetBookingDetailsAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Mentor)
                .Include(b => b.Mentee)
                .Include(b => b.Payments)  
                .Include(b => b.Reviews)
                .FirstOrDefaultAsync(b => b.BookingId == id && !b.IsDeleted.Value);
        }

        public async Task<string> GetGoogleMeetLinkAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Where(b => b.BookingId == bookingId && !b.IsDeleted.Value)
                .Select(b => b.GoogleMeetLink)
                .FirstOrDefaultAsync();

            return booking;
        }

        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            booking.CreatedAt = DateTime.Now;
            booking.Status = "Pending";
            booking.PaymentStatus = "Unpaid";
            booking.IsDeleted = false;

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Explicitly load the Mentor relationship
            await _context.Entry(booking)
                .Reference(b => b.Mentor)
                .LoadAsync();

            return booking;
        }
        public async Task<Booking> UpdateBookingAsync(Booking booking)
        {
            if (booking == null)
            {
                throw new ArgumentNullException(nameof(booking));
            }

            if (!await _context.Bookings.AnyAsync(b => b.BookingId == booking.BookingId))
            {
                throw new KeyNotFoundException($"Booking with ID {booking.BookingId} not found");
            }
            try
            {
                booking.UpdatedAt = DateTime.Now;  
                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();
                return booking;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking with ID {BookingId}", booking.BookingId);
                throw;  
            }
        }

        public async Task<bool> DeleteBookingAsync(int id)  
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return false;

            booking.IsDeleted = true;
            booking.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> SoftDeleteBookingAsync(int id)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(id);
                if (booking == null || booking.IsDeleted.GetValueOrDefault())
                {
                    return false;
                }

                // Perform soft delete
                booking.IsDeleted = true;
                booking.UpdatedAt = DateTime.UtcNow;
                booking.Status = "Cancelled"; // Optionally update status

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting booking {BookingId}", id);
                throw;
            }
        }
    }
 }
