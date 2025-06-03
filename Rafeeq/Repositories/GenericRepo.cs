using Rafeeq.Models;

namespace Rafeeq.Repositories
{
    public class GenericRepo<TEntity> where TEntity : class
    {
        private RafeeqContext _context;

        public GenericRepo(RafeeqContext context) {
        _context = context;

        }
        // get by id generic
        public TEntity GetById(int id)
        {
            return _context.Set<TEntity>().Find(id);
        }

        // get all generic
        public List<TEntity> GetAll()
        {
            return _context.Set<TEntity>().ToList();
        }

        public void Add(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
            _context.SaveChanges();
        }

        public void Update(TEntity entity)
        {
            _context.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            save();
        }

        public void Delete(int id)
        {
         TEntity t = GetById(id);
            if (t != null)
            {
                _context.Set<TEntity>().Remove(t);
                save();
            }
        }

        public void save() { 
        _context.SaveChanges();
        }



    }
}
