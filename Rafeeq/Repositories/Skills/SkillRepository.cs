using Microsoft.EntityFrameworkCore;
using Rafeeq.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rafeeq.Repositories.Skills
{
    public class SkillRepository
    {
        private readonly RafeeqContext _context;

        public SkillRepository(RafeeqContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Skill>> GetAllAsync()
        {
            return await _context.Skills.ToListAsync();
        }

        public async Task<Skill> GetByIdAsync(int id)
        {
            return await _context.Skills.FindAsync(id);
        }

        public async Task<Skill> AddAsync(Skill skill)
        {
            await _context.Skills.AddAsync(skill);
            return skill;
        }

        public void Update(Skill skill)
        {
            _context.Skills.Update(skill);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var skill = await _context.Skills.FindAsync(id);
            if (skill == null)
                return false;

            _context.Skills.Remove(skill);
            return true;
        }

        public async Task<bool> SkillExistsAsync(string name)
        {
            return await _context.Skills.AnyAsync(s => s.Name == name);
        }

        public async Task<bool> SkillExistsAsync(int id)
        {
            return await _context.Skills.AnyAsync(s => s.SkillId == id);
        }
    }
}
