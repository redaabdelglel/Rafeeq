
using Microsoft.EntityFrameworkCore;
using Rafeeq.Models;
using Rafeeq.Repositories.RepositoryBase;

namespace Rafeeq.Repositories.Users
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(RafeeqContext Context) : base(Context) { 
        
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        //get by id
        public async Task<User> GetByIdAsync(int id)
        {
            //return await _context.Users.Include(d => d.Role).FirstOrDefaultAsync(u => u.UserId == id);
            return await Context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByExternalIdAndTypeAsync(string externalId, string externalType)
        //get all 
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            //return await _context.Users.Include(d=> d.Role).ToListAsync();
            return await Context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.ExternalId == externalId && u.ExternalType == externalType);
            return await _context.Users.Include(d => d.Role).ToListAsync();
        }

        public async Task<User?> GetUserWithRoleAsync(int userId)
        {
            return await Context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userId);
        }

        // update user
        //public void Update(User user)
        //{
        //    _context.Users.Update(user);
        //}

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
