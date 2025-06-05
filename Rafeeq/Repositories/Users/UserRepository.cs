
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
        //get by id
        public async Task<User> GetByIdAsync(int id)
        {
            return await _context.Users.Include(d => d.Role).FirstOrDefaultAsync(u => u.UserId == id);
        }
        //get all 
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.Include(d => d.Role).ToListAsync();
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

        // create user
        public async Task<IEnumerable<User>> AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return await _context.Users.Include(d => d.Role).ToListAsync();
        }

        // delete user 
        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;

        }
    }
}
