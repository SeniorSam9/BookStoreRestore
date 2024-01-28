using System.Linq.Expressions;
using BookStore.DataAccess.Data;
using BookStore.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace BookStore.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _dbContext;
        // dbSet acts in place of T "if T is category then dbSet is category "
        internal readonly DbSet<T> dbSet;
        public Repository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            dbSet = _dbContext.Set<T>();
        }

        public void Add(T entity)
        {
            _dbContext.Add(entity);
            _dbContext.SaveChanges();
        }

        public void Delete(T entity)
        {
            _dbContext.Remove(entity);
            _dbContext.SaveChanges();
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            _dbContext.RemoveRange(entities);
            _dbContext.SaveChanges();
        }

        public T Get(Expression<Func<T, bool>> filter)
        {
            // if dbset is category then query will be too
            IQueryable<T> query = dbSet;
            query = query.Where(filter);
            return query.FirstOrDefault();
        }

        public IEnumerable<T> GetAll()
        {
            IQueryable<T> query = dbSet;
            return query.ToList();


        }
    }
}
