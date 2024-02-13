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
        void Save();
    }
}