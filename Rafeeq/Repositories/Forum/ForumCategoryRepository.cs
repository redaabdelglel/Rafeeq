
using Rafeeq.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rafeeq.Repositories.Forum
{
    public class ForumCategoryRepository : IForumCategoryRepository
    {
        private readonly RafeeqContext _context;
        public ForumCategoryRepository(RafeeqContext context) { _context = context; }

        public async Task<List<ForumCategory>> GetAllWithPostCountAsync()
        {
            return await _context.ForumCategories
                .Include(c => c.Posts)
                .ToListAsync();
        }

        public async Task<ForumCategory?> GetByIdAsync(int id)
        {
            return await _context.ForumCategories
                .Include(c => c.Posts)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
        }

        public async Task AddAsync(ForumCategory category)
        {
            await _context.ForumCategories.AddAsync(category);
        }

        public void Update(ForumCategory category)
        {
            _context.ForumCategories.Update(category);
        }

        public void Delete(ForumCategory category)
        {
            _context.ForumCategories.Remove(category);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
