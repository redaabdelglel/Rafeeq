
using Microsoft.EntityFrameworkCore;
using Rafeeq.Models;

namespace Rafeeq.Repositories.Users
{
    public class UserRepository
    {
        private readonly RafeeqContext _context;

        public UserRepository(RafeeqContext context)
        {
            _context = context;
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _context.Users.Include(d => d.Role).FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.Include(d=> d.Role).ToListAsync();
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        // update user
        public void Update(User user)
        {
            _context.Users.Update(user);
        }

    }
}
