using Rafeeq.Models;
using Rafeeq.Repositories.RepositoryBase;

namespace Rafeeq.Repositories.Auth
{
    public interface IUserTokenRepository: IRepositoryBase<UserToken>
    {
        Task<UserToken?> GetTokenByValueAndTypeAsync(string tokenValue, string tokenType);
        Task<List<UserToken>> GetActiveTokensForUserAsync(int userId, string tokenType);
        Task<List<UserToken>> GetTokensForUserInTimeRange(int userId, string tokenType, DateTime sinceDateTime); // Add this method


    }
}
