using Microsoft.EntityFrameworkCore;
using Rafeeq.Models;
using Rafeeq.Repositories.RepositoryBase;

namespace Rafeeq.Repositories.Auth
{
    public class UserTokenRepository : RepositoryBase<UserToken>, IUserTokenRepository
    {
        public UserTokenRepository(RafeeqContext Context) : base(Context) { }

        public async Task<List<UserToken>> GetActiveTokensForUserAsync(int userId, string tokenType)
        {
            return await Context.UserTokens
                .Where(t => t.UserId == userId && t.TokenType == tokenType && t.IsUsed == false && t.ExpiryDate > DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<UserToken?> GetTokenByValueAndTypeAsync(string tokenValue, string tokenType)
        {
            return await Context.UserTokens
                .Where(t => t.TokenValue == tokenValue.Trim() && t.TokenType == tokenType && t.IsUsed == false && t.ExpiryDate > DateTime.UtcNow)
                .FirstOrDefaultAsync();
        }

        public async Task<List<UserToken>> GetTokensForUserInTimeRange(int userId, string tokenType, DateTime sinceDateTime)
        {
            return await Context.UserTokens
                .Where(t => t.UserId == userId &&
                           t.TokenType == tokenType &&
                           t.CreatedAt >= sinceDateTime)
                .ToListAsync();
        }

    }
}
