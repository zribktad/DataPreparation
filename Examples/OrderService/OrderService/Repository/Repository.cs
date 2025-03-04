using Microsoft.EntityFrameworkCore;
using OrderService.Models;

namespace OrderService.Repository
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly OrderServiceContext _context;

        public Repository(OrderServiceContext context)
        {
            _context = context;
        }

        public IEnumerable<T> GetAll(Func<IQueryable<T>, IQueryable<T>> include = null)
        {
            IQueryable<T> query = _context.Set<T>();

            if (include != null)
            {
                query = include(query);
            }

            return query.ToList();
        }

        public T GetById(long id, Func<IQueryable<T>, IQueryable<T>> include = null)
        {
            IQueryable<T> query = _context.Set<T>();

            if (include != null)
            {
                query = include(query);
            }

            return query.FirstOrDefault(entity => entity.Id == id);
        }
        public T Insert(T entity)
        {
            var res =_context.Set<T>().Add(entity);
            _context.SaveChanges();
            return res.Entity;
        }

        public T Update(T entity)
        {
            var res = _context.Set<T>().Update(entity);
            _context.SaveChanges();
            return res.Entity;
        }

        public void Delete(long id)
        {
            var entity = GetById(id);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}