using BookStore.DataAccess.Data;
using BookStore.DataAccess.Repository;
using BookStore.Models;
using BookStore.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;



namespace BulkyBook.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private ApplicationDbContext _dbContext;
        public ShoppingCartRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }



        public void Update(ShoppingCart obj)
        {
            _dbContext.ShoppingCarts.Update(obj);
        }
    }
}