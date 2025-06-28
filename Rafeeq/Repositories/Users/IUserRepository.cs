using Rafeeq.Models;
using Rafeeq.Repositories.RepositoryBase;

namespace Rafeeq.Repositories.Users
{
    public interface IUserRepository : IRepositoryBase<User>
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserWithRoleAsync(int userId);
        Task<User?> GetUserByExternalIdAndTypeAsync(string externalId, string externalType);


        //userPrfile
        new Task<User?> GetByIdAsync(int id);
        new Task<IEnumerable<User>> GetAllAsync();
    }
}
