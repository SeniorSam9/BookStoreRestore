using BookStore.Models;
using Microsoft.EntityFrameworkCore;
// this file is the gateway between the this project and the database
namespace BookStore.DataAccess.Data
{
    public class ApplicationDbContext : DbContext
    {
        // whatever configuration we do in the app db context
        // we will pass them to the DbContext
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContextOptions)
        : base(dbContextOptions)
        {

        }
        // this line alone is enough to create a table with name (Categories) in the db
        public DbSet<Category> Categories { get; set; }

        // model builder allows us to deal with the data exists in the DB
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // here is a new update
            // hence we need to add migration
            modelBuilder.Entity<Category>().HasData(new Category
            {
                Id = 1,
                Name = "Action",
                DisplayOrder = 1,
            });
        }
    }
}
