using Microsoft.EntityFrameworkCore;
using restaurant.Models;

namespace restaurant.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions dbContextOptions)
            : base(dbContextOptions)
        {
        }

        public DbSet<MenuItem> MenuItems { get; set; }
    }
}