
using Microsoft.EntityFrameworkCore;
using Rafeeq.DTOs.Skills;
using Rafeeq.DTOs.Users;
using Rafeeq.Models;
using Rafeeq.Repositories.RepositoryBase;

namespace Rafeeq.Repositories.Users
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(RafeeqContext Context) : base(Context) { 
        
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await Context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email);
        }

        //get by id
        public async Task<User> GetByIdAsync(int id)
        {
            return await Context.Users.Include(d => d.Role).FirstOrDefaultAsync(u => u.UserId == id);
           
        }

        public async Task<User?> GetUserByExternalIdAndTypeAsync(string externalId, string externalType)
        {
            //return await _context.Users.Include(d=> d.Role).ToListAsync();
            return await Context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.ExternalId == externalId && u.ExternalType == externalType);
        }   



        //get all users
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await Context.Users.Include(d=> d.Role).ToListAsync();
            
        }


        public async Task<User?> GetUserWithRoleAsync(int userId)
        {
            return await Context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userId);
        }

        //update user
        public void Update(User user)
        {
            Context.Users.Update(user);
        }

        // create user
        public async Task<IEnumerable<User>> AddAsync(User user)
        {
            await Context.Users.AddAsync(user);
            await Context.SaveChangesAsync();
            return await Context.Users.Include(d => d.Role).ToListAsync();
        }

        // delete user 
        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await Context.Users.FindAsync(id);
            if (user == null)
                return false;
            Context.Users.Remove(user);
            await Context.SaveChangesAsync();
            return true;

        }
        //get all mentors with their skills
        public async Task<IEnumerable<MentorDto>> GetAllMentors()
        {
            var mentors = await Context.Users
                .Where(u => u.IsMentor == true && u.IsDeleted == false)
                .Include(u => u.MentorSkills)
                .ThenInclude(ms => ms.Skill)
                .Select(u => new MentorDto
                {
                    Id = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    role = u.Role.RoleName,
                    HourlyRate = u.HourlyRate ?? 0,
                    MentorSkills = u.MentorSkills.Select(ms => new SkillDto
                    {
                        Id = ms.Skill.SkillId,
                        Name = ms.Skill.Name
                    }).ToList()
                })
                .ToListAsync();

            return mentors;
        }


    }
}
