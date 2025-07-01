using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Rafeeq.DTOs.Bookings;
using Rafeeq.Models;
using Rafeeq.DTOs.Availability;

namespace Rafeeq.Repositories.Bookings
{
        public interface IMenteeBookingRepository
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
        Task<bool> IsSlotAvailableAsync(int mentorId, DateTime startDateTime, DateTime endDateTime);


    }

    public class MenteeBookingRepository : IMenteeBookingRepository
    {
            private readonly RafeeqContext _context;
            private readonly ILogger<MenteeBookingRepository> _logger;  
        public MenteeBookingRepository(RafeeqContext context)
        {
            _context = context;

        }
        public MenteeBookingRepository(RafeeqContext context, ILogger<MenteeBookingRepository> logger)
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

        public async Task<List<AvailabilityDto>> GetMentorAvailabilityAsync(int mentorId, int daysToLookAhead = 30)
        {
            var result = new List<AvailabilityDto>();
            var now = DateTime.UtcNow;

            // Get mentor with their availability and existing bookings
            var mentor = await _context.Users
                .Include(u => u.Availabilities)
                .Include(u => u.BookingMentors)  // Using BookingMentors as per your User model
                .FirstOrDefaultAsync(u => u.UserId == mentorId && u.IsMentor == true);

            if (mentor == null)
                return result;

            // Process each availability slot
            foreach (var availability in mentor.Availabilities.Where(a =>
                     a.DayOfWeek.HasValue &&
                     a.StartTime.HasValue &&
                     a.EndTime.HasValue))
            {
                var slot = new AvailabilityDto
                {
                    AvailabilityId = availability.AvailabilityId,
                    UserId = availability.UserId ?? 0,
                    DayOfWeek = availability.DayOfWeek.Value,
                    StartTime = availability.StartTime.Value,
                    EndTime = availability.EndTime.Value
                };

                // Generate available times for next X days
                for (int i = 0; i < daysToLookAhead; i++)
                {
                    var date = now.AddDays(i);

                    // Only process days matching the availability day
                    if ((int)date.DayOfWeek == availability.DayOfWeek)
                    {
                        var slotStart = date.Date.Add(availability.StartTime.Value);
                        var slotEnd = date.Date.Add(availability.EndTime.Value);

                        // Check if this time slot is booked
                        var isBooked = mentor.BookingMentors.Any(b =>
                            b.IsDeleted != true &&
                            b.Status != "Cancelled" &&
                            b.StartDateTime < slotEnd &&
                            b.EndDateTime > slotStart);

                        if (!isBooked && slotStart > now.AddMinutes(30))
                        {
                            // Since we're returning AvailabilityDto, we can't add available times here
                            // You might want to create a different DTO if you need available times
                        }
                    }
                }

                result.Add(slot);
            }

            return result;
        }
        // MenteeBookingRepository.cs
        public async Task<bool> IsSlotAvailableAsync(int mentorId, DateTime startDateTime, DateTime endDateTime)
        {
            // Get the mentor with their availabilities and existing bookings
            var mentor = await _context.Users
                .Include(u => u.Availabilities)
                .Include(u => u.BookingMentors)  // Using BookingMentors as per your model
                .FirstOrDefaultAsync(u => u.UserId == mentorId && u.IsMentor == true);

            if (mentor == null)
                return false;

            // Check if mentor has availability for this day/time
            var dayOfWeek = (int)startDateTime.DayOfWeek;
            var startTime = startDateTime.TimeOfDay;
            var endTime = endDateTime.TimeOfDay;

            var hasAvailability = mentor.Availabilities.Any(a =>
                a.DayOfWeek == dayOfWeek &&
                a.StartTime <= startTime &&
                a.EndTime >= endTime);

            if (!hasAvailability)
                return false;

            // Check for conflicting bookings
            var hasConflict = mentor.BookingMentors.Any(b =>
                b.IsDeleted != true &&
                b.Status != "Cancelled" &&
                b.StartDateTime < endDateTime &&
                b.EndDateTime > startDateTime);

            return !hasConflict;
        }
    }
 }
