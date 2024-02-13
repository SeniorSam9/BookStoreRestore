using BookStore.DataAccess.Data;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private static readonly char[] separator = new char[] {','};

        public ProductRepository(ApplicationDbContext dbContext) : base(dbContext) 
        {
            _dbContext = dbContext;
            _dbContext.Products.Include(p => p.Category);
        }

        public void Update(Product product)
        {
            _dbContext.Products.Update(product);
            _dbContext.SaveChanges();
        }

        public override Product Get(Expression<Func<Product, bool>> filter, string? includeProperties = null)
        {
            IQueryable<Product> query = getDbSet();
            query = query.Where(filter);

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var incProp in includeProperties.Split(
                    separator,
                    StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(incProp);
                }
            }

            return query.FirstOrDefault();
        }
        public override IEnumerable<Product> GetAll(string? includeProperties = null)
        {
            IQueryable<Product> query = getDbSet();

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var incProp in includeProperties.Split(
                    separator, 
                    StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(incProp);
                }
            }
            
            return query.ToList();

        }
    }
}
