// Repositories/Users/MentorRepository.cs
using Microsoft.EntityFrameworkCore;
using Rafeeq.DTOs.Availability;
using Rafeeq.DTOs.Users;
using Rafeeq.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rafeeq.Repositories.Users
{
    public interface IMentorRepository
    {
        Task<IEnumerable<User>> GetMentorsAsync(MentorFilterDTO filter);
        Task<User> GetMentorProfileAsync(int id);
        Task<IEnumerable<User>> GetAllMentorsAsync();
        Task<User?> GetMentorByIdAsync(int id);
        Task<List<AvailableSlotDto>> GetMentorAvailabilityAsync(int mentorId, int daysAhead = 14);
        Task<bool> IsTimeSlotAvailableAsync(int mentorId, DateTime start, DateTime end);



    }

    public class MenteeMentorRepository : IMentorRepository
    {
        private readonly RafeeqContext _context;

        public MenteeMentorRepository(RafeeqContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllMentorsAsync()
        {
            return await _context.Users
                .Include(u => u.MentorSkills)
                .ThenInclude(ms => ms.Skill)
                .Include(u => u.Availabilities)
                .Where(u => u.IsMentor == true && !(u.IsDeleted ?? false))
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetMentorsAsync(MentorFilterDTO filter)
        {
            var query = _context.Users
                .Include(u => u.MentorSkills)
                .ThenInclude(ms => ms.Skill)
                .Include(u => u.Availabilities)
                .Where(u => u.IsMentor == true && !(u.IsDeleted ?? false));

            if (!string.IsNullOrEmpty(filter.Skill))
            {
                query = query.Where(u => u.MentorSkills.Any(ms =>
                    ms.Skill.Name.Contains(filter.Skill)));
            }

            if (filter.MaxHourlyRate.HasValue)
            {
                query = query.Where(u => u.HourlyRate <= filter.MaxHourlyRate.Value);
            }

            if (!string.IsNullOrEmpty(filter.Name))
            {
                query = query.Where(u => u.FullName.Contains(filter.Name));
            }

            return await query.ToListAsync();
        }

        public async Task<User> GetMentorProfileAsync(int id)
        {
            var mentor = await _context.Users
                .Include(u => u.MentorSkills)
                .ThenInclude(ms => ms.Skill)
                .Include(u => u.Availabilities)
                .FirstOrDefaultAsync(u => u.UserId == id && u.IsMentor == true && !(u.IsDeleted ?? false));

            // DEBUG: Verify loaded data
            foreach (var a in mentor?.Availabilities)
            {
                Console.WriteLine($"DB Values - ID: {a.AvailabilityId}, " +
                                 $"DayOfWeek: {a.DayOfWeek}, " +
                                 $"Type: {a.DayOfWeek?.GetType()}");
            }

            return mentor;
        }


        // Add to MenteeMentorRepository class
        public async Task<User?> GetMentorByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.MentorSkills)
                    .ThenInclude(ms => ms.Skill)
                .Include(u => u.Availabilities)
                .Include(u => u.ReviewReviewers)
                    .ThenInclude(r => r.Reviewer)
                .Where(u => u.UserId == id &&
                           u.IsMentor == true &&
                           !(u.IsDeleted ?? false))
                .AsSplitQuery() 
                .FirstOrDefaultAsync();
        }

        public async Task<List<AvailableSlotDto>> GetMentorAvailabilityAsync(int mentorId, int daysAhead = 14)
        {
            var result = new List<AvailableSlotDto>();
            var now = DateTime.UtcNow;
            var endDate = now.AddDays(daysAhead);

            // Get mentor with their availability and bookings
            var mentor = await _context.Users
                .Include(u => u.Availabilities)
                .Include(u => u.BookingMentors)  // Changed from BookingsAsMentor to match your model
                .FirstOrDefaultAsync(u => u.UserId == mentorId && u.IsMentor == true);

            if (mentor == null) return result;

            // Process each availability slot
            foreach (var availability in mentor.Availabilities.Where(a =>
                     a.DayOfWeek.HasValue &&
                     a.StartTime.HasValue &&
                     a.EndTime.HasValue &&
                     a.StartTime < a.EndTime))
            {
                var slot = new AvailableSlotDto
                {
                    DayOfWeek = availability.DayOfWeek.Value,
                    DayName = Enum.GetName(typeof(DayOfWeek), availability.DayOfWeek.Value) ?? "Unknown",
                    StartTime = availability.StartTime.Value,
                    EndTime = availability.EndTime.Value
                };

                // Generate available times for each matching day in the date range
                for (var date = now.Date; date <= endDate; date = date.AddDays(1))
                {
                    if ((int)date.DayOfWeek == availability.DayOfWeek.Value)
                    {
                        var slotStart = date.Add(availability.StartTime.Value);
                        var slotEnd = date.Add(availability.EndTime.Value);

                        // Check if this time slot is available
                        if (!IsBooked(mentor.BookingMentors, slotStart, slotEnd) &&
                            slotStart > now.AddMinutes(30))
                        {
                            slot.AvailableSlots.Add(slotStart);
                        }
                    }
                }

                if (slot.AvailableSlots.Any())
                {
                    result.Add(slot);
                }
            }

            return result;
        }

        private bool IsBooked(IEnumerable<Booking> bookings, DateTime start, DateTime end)
        {
            return bookings.Any(b =>
                b.IsDeleted != true &&         
                b.Status != "Cancelled" &&     
                b.StartDateTime < end &&       
                b.EndDateTime > start);        
        }
        public async Task<bool> IsTimeSlotAvailableAsync(int mentorId, DateTime start, DateTime end)
        {
            var mentor = await _context.Users
                .Include(u => u.Availabilities)
                .Include(u => u.BookingMentors)
                .FirstOrDefaultAsync(u => u.UserId == mentorId && u.IsMentor == true);

            if (mentor == null) return false;

            // Check availability for the day and time
            var hasAvailability = mentor.Availabilities.Any(a =>
                a.DayOfWeek == (int)start.DayOfWeek &&
                a.StartTime <= start.TimeOfDay &&
                a.EndTime >= end.TimeOfDay);

            if (!hasAvailability) return false;

            // Check for booking conflicts
            return !IsBooked(mentor.BookingMentors, start, end);
        }

    }
}
