using full.API.Models;
using Microsoft.EntityFrameworkCore;

namespace full.API.Data
{
    public class FullDbContext : DbContext
    {
        public FullDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
    }
}
