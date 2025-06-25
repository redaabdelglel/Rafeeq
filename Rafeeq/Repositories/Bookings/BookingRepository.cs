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

        public async Task<bool> HasBookingsForAvailabilityAsync(int mentorId, int dayOfWeek, TimeSpan startTime, TimeSpan endTime)
        {
            // Get the current date
            DateTime now = DateTime.UtcNow.Date;

            // Look ahead up to 3 months for bookings
            DateTime endDate = now.AddMonths(3);

            // Loop through all dates in the next 3 months
            for (DateTime date = now; date <= endDate; date = date.AddDays(1))
            {
                // Check if this date matches the day of week we're checking
                if ((int)date.DayOfWeek == dayOfWeek)
                {
                    // Create datetime for start and end of this slot on this date
                    DateTime slotStartDateTime = date.Add(startTime);
                    DateTime slotEndDateTime = date.Add(endTime);

                    // Look for any bookings that overlap with this slot
                    bool hasBooking = await _context.Bookings
                        .AnyAsync(b => b.MentorId == mentorId
                                   && b.Status != "Cancelled"
                                   && b.IsDeleted != true
                                   && ((slotStartDateTime <= b.StartDateTime && b.StartDateTime < slotEndDateTime) ||
                                       (slotStartDateTime < b.EndDateTime && b.EndDateTime <= slotEndDateTime)));

                    if (hasBooking)
                        return true;
                }
            }

            return false;
        }

        // Get bookings for a specific mentor
        public async Task<IEnumerable<BookingDto>> GetBookingsByMentorIdAsync(int mentorId)
        {
            return await _context.Bookings
                .Include(b => b.Mentor)
                .Include(b => b.Mentee)
                .Where(b => b.MentorId == mentorId && b.IsDeleted == false)
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

        // Get booking by ID
        public async Task<Booking> GetByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Mentor)
                .Include(b => b.Mentee)
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }

        // Update booking status
        public void UpdateStatus(Booking booking)
        {
            booking.UpdatedAt = DateTime.UtcNow;
            _context.Bookings.Update(booking);
        }

        // Add the missing Update method
        public void Update(Booking booking)
        {
            booking.UpdatedAt = DateTime.UtcNow;
            _context.Bookings.Update(booking);

        }
       

        // Get upcoming bookings for mentor
        public async Task<IEnumerable<Booking>> GetUpcomingMentorBookingsAsync(int mentorId)
        {
            var currentTime = DateTime.UtcNow;
            return await _context.Bookings
                .Include(b => b.Mentor)
                .Include(b => b.Mentee)
                .Where(b => b.MentorId == mentorId
                        && b.IsDeleted != true
                        && b.StartDateTime > currentTime
                        && (b.Status == "Confirmed" || b.Status == "Pending"))
                .OrderBy(b => b.StartDateTime)
                .ToListAsync();
        }

        // Get completed bookings for mentor
        public async Task<IEnumerable<Booking>> GetCompletedMentorBookingsAsync(int mentorId)
        {
            return await _context.Bookings
                .Include(b => b.Mentor)
                .Include(b => b.Mentee)
                .Where(b => b.MentorId == mentorId
                        && b.IsDeleted != true
                        && b.Status == "Completed")
                .OrderByDescending(b => b.EndDateTime)
                .ToListAsync();
        }

        // Get upcoming bookings for mentee 
        public async Task<IEnumerable<Booking>> GetUpcomingMenteeBookingsAsync(int menteeId)
        {
            var currentTime = DateTime.UtcNow;
            return await _context.Bookings
                .Include(b => b.Mentor)
                .Include(b => b.Mentee)
                .Where(b => b.MenteeId == menteeId
                        && b.IsDeleted != true
                        && b.StartDateTime > currentTime
                        && (b.Status == "Confirmed" || b.Status == "Pending"))
                .OrderBy(b => b.StartDateTime)
                .ToListAsync();
        }

        // Get completed bookings for mentee
        public async Task<IEnumerable<Booking>> GetCompletedMenteeBookingsAsync(int menteeId)
        {
            return await _context.Bookings
                .Include(b => b.Mentor)
                .Include(b => b.Mentee)
                .Where(b => b.MenteeId == menteeId
                        && b.IsDeleted != true
                        && b.Status == "Completed")
                .OrderByDescending(b => b.EndDateTime)
                .ToListAsync();
        }

        // Check for overlapping bookings (excluding current booking)
        public async Task<bool> HasOverlappingBookingsAsync(
            int mentorId, DateTime startDateTime, DateTime endDateTime, int excludeBookingId = 0)
        {
            return await _context.Bookings
                .AnyAsync(b => b.MentorId == mentorId
                          && b.BookingId != excludeBookingId
                          && b.IsDeleted != true
                          && b.Status != "Cancelled"
                          && ((startDateTime <= b.StartDateTime && b.StartDateTime < endDateTime) ||
                              (startDateTime < b.EndDateTime && b.EndDateTime <= endDateTime) ||
                              (b.StartDateTime <= startDateTime && endDateTime <= b.EndDateTime)));
        }


        
        public async Task<Booking> GetBookingWithParticipantsAsync(int bookingId)
        {
            return await _context.Bookings
                .Include(b => b.Mentor)
                .Include(b => b.Mentee)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.IsDeleted != true);
        }

        // Get all bookings for a user (either as mentor or mentee)
        public async Task<IEnumerable<Booking>> GetBookingsForUserAsync(int userId)
        {
            return await _context.Bookings
                .Include(b => b.Mentor)
                .Include(b => b.Mentee)
                .Where(b => (b.MentorId == userId || b.MenteeId == userId) && b.IsDeleted == false)
                .ToListAsync();
        }


    }
}
