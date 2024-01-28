using BookStore.DataAccess.Data;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ProductRepository(ApplicationDbContext dbContext) : base(dbContext) 
        {
            _dbContext = dbContext;
        }

        public void Update(Product product)
        {
            _dbContext.Products.Update(product);
            _dbContext.SaveChanges();
        }
    }
}
