using Microsoft.EntityFrameworkCore;
using Rafeeq.Models;
using System.Linq.Expressions;

namespace Rafeeq.Repositories
{
    public class RoleRepository 
    {

        private readonly RafeeqContext context;

        public RoleRepository(RafeeqContext context)
        {
            this.context = context;
        }

        public async Task<Role?> GetByCondition(Expression<Func<Role, bool>> predicate)
        {
            return await context.Roles.FirstOrDefaultAsync(predicate);
        }
    
    }
}
