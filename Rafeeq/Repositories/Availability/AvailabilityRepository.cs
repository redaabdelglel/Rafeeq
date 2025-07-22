using Microsoft.EntityFrameworkCore;
using Rafeeq.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rafeeq.Repositories.Availability
{
    public class AvailabilityRepository
    {
        private readonly RafeeqContext _context;

        public AvailabilityRepository(RafeeqContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Models.Availability>> GetByUserIdAsync(int userId)
        {
            return await _context.Availabilities
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.DayOfWeek)
                .ThenBy(a => a.StartTime)
                .ToListAsync();
        }

        public async Task<Models.Availability> GetByIdAsync(int id)
        {
            return await _context.Availabilities.FindAsync(id);
        }

        public async Task<Models.Availability> AddAsync(Models.Availability availability)
        {
            await _context.Availabilities.AddAsync(availability);
            return availability;
        }

        public void Update(Models.Availability availability)
        {
            _context.Availabilities.Update(availability);
        }

        public void Delete(Models.Availability availability)
        {
            _context.Availabilities.Remove(availability);
        }

        public async Task<bool> HasOverlappingAvailabilityAsync(Models.Availability availability)
        {
            return await _context.Availabilities
                .AnyAsync(a => a.UserId == availability.UserId
                          && a.DayOfWeek == availability.DayOfWeek
                          && a.AvailabilityId != availability.AvailabilityId
                          && ((a.StartTime <= availability.StartTime && availability.StartTime < a.EndTime) ||
                              (a.StartTime < availability.EndTime && availability.EndTime <= a.EndTime) ||
                              (availability.StartTime <= a.StartTime && a.StartTime < availability.EndTime)));
        }
    }
}
