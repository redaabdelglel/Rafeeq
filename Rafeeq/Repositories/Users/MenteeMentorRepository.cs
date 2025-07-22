
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

        Task<List<TimeSlotDto>> GetAvailableTimeSlotsAsync(int mentorId, DateTime startDate, DateTime endDate);
        Task<List<AvailableSlotDto>> GetTrueAvailableSlotsAsync(int mentorId, DateTime startDate, DateTime endDate);
        Task<List<TimeSlotDto>> GetOnlyFreeTimeSlots(int mentorId);


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

           
            foreach (var a in mentor?.Availabilities)
            {
                Console.WriteLine($"DB Values - ID: {a.AvailabilityId}, " +
                                 $"DayOfWeek: {a.DayOfWeek}, " +
                                 $"Type: {a.DayOfWeek?.GetType()}");
            }

            return mentor;
        }


       
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

            var mentor = await _context.Users
                .Include(u => u.Availabilities)
                .Include(u => u.BookingMentors)  
                .FirstOrDefaultAsync(u => u.UserId == mentorId && u.IsMentor == true);

            if (mentor == null) return result;

        
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

               
                for (var date = now.Date; date <= endDate; date = date.AddDays(1))
                {
                    if ((int)date.DayOfWeek == availability.DayOfWeek.Value)
                    {
                        var slotStart = date.Add(availability.StartTime.Value);
                        var slotEnd = date.Add(availability.EndTime.Value);

                        
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
         
            if (start >= end)
                return false;

           
            var mentor = await _context.Users
                .Include(u => u.Availabilities)
                .Include(u => u.BookingMentors)
                .FirstOrDefaultAsync(u => u.UserId == mentorId && u.IsMentor == true);

            if (mentor == null)
                return false;

            
            var dayOfWeek = (int)start.DayOfWeek;
            var startTime = start.TimeOfDay;
            var endTime = end.TimeOfDay;

            var hasAvailability = mentor.Availabilities.Any(a =>
                a.DayOfWeek.HasValue &&
                a.DayOfWeek.Value == dayOfWeek &&
                a.StartTime.HasValue &&
                a.EndTime.HasValue &&
                a.StartTime.Value <= startTime &&
                a.EndTime.Value >= endTime);

            if (!hasAvailability)
                return false;

           
            var hasConflict = mentor.BookingMentors.Any(b =>
                !b.IsDeleted.GetValueOrDefault() &&
                b.Status != "Cancelled" &&
                b.StartDateTime.HasValue &&  
                b.EndDateTime.HasValue &&    
                b.StartDateTime.Value.Date == start.Date &&  
                b.StartDateTime.Value < end &&
                b.EndDateTime.Value > start);

            return !hasConflict;
        }
        public async Task<List<AvailableSlotDto>> GetTrueAvailableSlotsAsync(int mentorId, DateTime startDate, DateTime endDate)
        {
            var result = new List<AvailableSlotDto>();

            var mentor = await _context.Users
                .Include(u => u.Availabilities)
                .Include(u => u.BookingMentors)
                .FirstOrDefaultAsync(u => u.UserId == mentorId && u.IsMentor == true);

            if (mentor == null) return result;

            var existingBookings = mentor.BookingMentors
                .Where(b => !b.IsDeleted.GetValueOrDefault() && b.Status != "Cancelled")
                .ToList();

            foreach (var availability in mentor.Availabilities
                .Where(a => a.DayOfWeek.HasValue && a.StartTime.HasValue && a.EndTime.HasValue))
            {
                var dayOfWeek = availability.DayOfWeek.Value;
                var dayName = Enum.GetName(typeof(DayOfWeek), dayOfWeek) ?? "Unknown";
                var startTime = availability.StartTime.Value;
                var endTime = availability.EndTime.Value;

                var slotDto = new AvailableSlotDto
                {
                    DayOfWeek = dayOfWeek,
                    DayName = dayName,
                    StartTime = startTime,
                    EndTime = endTime
                };

                for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    if ((int)date.DayOfWeek == dayOfWeek)
                    {
                        var slotStart = date.Add(startTime);
                        var slotEnd = date.Add(endTime);

                      
                        var isBooked = existingBookings.Any(b =>
                            b.StartDateTime < slotEnd &&
                            b.EndDateTime > slotStart);

                        if (!isBooked && slotStart > DateTime.UtcNow.AddMinutes(30))
                        {
                            slotDto.AvailableSlots.Add(slotStart);
                        }
                    }
                }

                if (slotDto.AvailableSlots.Any())
                {
                    result.Add(slotDto);
                }
            }

            return result;
        }
        public async Task<List<TimeSlotDto>> GetAvailableTimeSlotsAsync(int mentorId, DateTime startDate, DateTime endDate)
        {
            var availableSlots = await GetTrueAvailableSlotsAsync(mentorId, startDate, endDate);
            var result = new List<TimeSlotDto>();

            foreach (var slot in availableSlots)
            {
                foreach (var availableTime in slot.AvailableSlots)
                {
                    result.Add(new TimeSlotDto
                    {
                        Start = availableTime,
                        End = availableTime.Add(slot.EndTime - slot.StartTime)
                    });
                }
            }

            return result.OrderBy(ts => ts.Start).ToList();
        }
        public async Task<List<TimeSlotDto>> GetOnlyFreeTimeSlots(int mentorId)
        {
            var result = new List<TimeSlotDto>();
            var now = DateTime.UtcNow;

            
            var mentor = await _context.Users
                .Include(u => u.Availabilities)
                .Include(u => u.BookingMentors)
                .FirstOrDefaultAsync(u => u.UserId == mentorId && u.IsMentor == true);

            if (mentor == null) return result;

           
            var existingBookings = mentor.BookingMentors
                .Where(b => !b.IsDeleted.GetValueOrDefault() &&
                           b.Status != "Cancelled" &&
                           b.EndDateTime > now)
                .ToList();

           
            foreach (var availability in mentor.Availabilities
                .Where(a => a.DayOfWeek.HasValue && a.StartTime.HasValue && a.EndTime.HasValue))
            {
                var dayOfWeek = availability.DayOfWeek.Value;
                var startTime = availability.StartTime.Value;
                var endTime = availability.EndTime.Value;

                for (int i = 0; i < 30; i++)
                {
                    var date = now.Date.AddDays(i);
                    if ((int)date.DayOfWeek != dayOfWeek) continue;

                    var slotStart = date.Add(startTime);
                    var slotEnd = date.Add(endTime);

                    
                    if (!existingBookings.Any(b =>
                        b.StartDateTime < slotEnd &&
                        b.EndDateTime > slotStart) &&
                        slotStart > now.AddMinutes(30)) 
                    {
                        result.Add(new TimeSlotDto
                        {
                            Start = slotStart,
                            End = slotEnd,
                        });
                    }
                }
            }

            return result.OrderBy(s => s.Start).ToList();
        }
       

    }
}
