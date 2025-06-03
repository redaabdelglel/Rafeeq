using Microsoft.EntityFrameworkCore;
using Rafeeq.DTOs.Users;
using Rafeeq.Models;

namespace Rafeeq.Repositories.Users
{
    public interface IMentorRepository
    {
        Task<IEnumerable<User>> GetMentorsAsync(MentorFilterDTO filter);
        Task<User> GetMentorProfileAsync(int id);
    }

    public class MentorRepository : IMentorRepository
    {
        private readonly RafeeqContext _context;

        public MentorRepository(RafeeqContext context)
        {
            _context = context;
            

        }

        public async Task<IEnumerable<User>> GetMentorsAsync(MentorFilterDTO filter)
        {
            var query = _context.Users
                .Include(u => u.MentorSkills)
                .ThenInclude(ms => ms.Skill)
                .Where(u => u.IsMentor == true && !u.IsDeleted.Value);

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
                .FirstOrDefaultAsync(u => u.UserId == id && u.IsMentor == true && !u.IsDeleted.Value);
        }
    }
}
