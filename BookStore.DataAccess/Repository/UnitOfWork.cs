using BookStore.DataAccess.Data;
using BookStore.DataAccess.Repository.IRepository;

namespace BookStore.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        public ICategoryRepository Category { get; private set; }

        private readonly ApplicationDbContext _dbContext;

        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            Category = new CategoryRepository(_dbContext);
        }
        public void Save()
        {
            _dbContext.SaveChanges();
        }
    }
}