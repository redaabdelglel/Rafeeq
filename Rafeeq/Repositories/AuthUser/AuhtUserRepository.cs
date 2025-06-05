using Rafeeq.Models;
using Rafeeq.Repositories.RepositoryBase;
using static Rafeeq.Repositories.AuthUser.IAuhtUserRepository;

namespace Rafeeq.Repositories.AuthUser
{
//    public class AuhtUserRepository::RepositoryBase<User>, IUserRepository
//    {
//        public AuhtUserRepository(RafeeqContext context) : base(context) { }
//    public async Task<User?> GetUserByEmailAsync(string email)
//    {
//        //return await _context.Users.Include(d => d.Role).FirstOrDefaultAsync(u => u.UserId == id);
//        return await Context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email);
//    }

//    public async Task<User?> GetUserByExternalIdAndTypeAsync(string externalId, string externalType)
//    {
//        //return await _context.Users.Include(d=> d.Role).ToListAsync();
//        return await Context.Users
//            .Include(u => u.Role)
//            .FirstOrDefaultAsync(u => u.ExternalId == externalId && u.ExternalType == externalType);
//    }

//    public async Task<User?> GetUserWithRoleAsync(int userId)
//    {
//        return await Context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userId);
//    }

//    // update user
//    //public void Update(User user)
//    //{
//    //    _context.Users.Update(user);
//    //}
//}
}
