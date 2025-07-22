using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Rafeeq.Models;

namespace Rafeeq.Repositories.RepositoryBase
{
    public class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        public  RafeeqContext Context;
        public RepositoryBase(RafeeqContext Context) {
        this.Context = Context;
        }

        public void Add(T entity)
        {
         Context.Set<T>().Add(entity);
        }

        public void AddRange(IEnumerable<T> entities)
        {
         Context.Set<T>().AddRange(entities);
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> expression)
        {
            return Context.Set<T>().Where(expression);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {

            return await Context.Set<T>().ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
             return await Context.Set<T>().FindAsync(id);
        }

     


        public IQueryable<T> GetQuery()
        {
            return Context.Set<T>().AsQueryable();
        }


        public void Remove(T entity)
        {
          Context.Set<T>().Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
              Context.Set<T>().RemoveRange(entities);

        }

        public void Update(T entity)
        {
            Context.Set<T>().Update(entity);
        }
    }
}
