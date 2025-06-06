// Repositories/Users/MentorRepository.cs
using Microsoft.EntityFrameworkCore;
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
    }

    public class MentorRepository : IMentorRepository
    {
        private readonly RafeeqContext _context;

        public MentorRepository(RafeeqContext context)
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
            return await _context.Users
                .Include(u => u.MentorSkills)
                .ThenInclude(ms => ms.Skill)
                .Include(u => u.Availabilities)
              //  .Include(u => u.Reviews)
                .FirstOrDefaultAsync(u => u.UserId == id && u.IsMentor == true && !(u.IsDeleted ?? false));
        }
    }
}