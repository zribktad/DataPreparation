namespace OrderService.Repository
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll(Func<IQueryable<T>, IQueryable<T>> include = null);
        T GetById(long id, Func<IQueryable<T>, IQueryable<T>> include = null);
        T Insert(T entity);
        T Update(T entity);
        void Delete(long id);
    }
}
