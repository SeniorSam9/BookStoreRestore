/*
UnitOfWork is not necessary but it helps organizing Repos
*/

namespace BookStore.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        // you cant set a value in an interface
        ICategoryRepository Category { get; }
        IProductRepository Product { get; }
        ICompanyRepository Company { get; }
        IShoppingCartRepository ShoppingCart { get; }
        IApplicationUserRepository ApplicationUser { get; }
        IOrderDetailRepository OrderDetail { get; }
        IOrderHeaderRepository OrderHeader { get; }
        void Save();
    }
}
