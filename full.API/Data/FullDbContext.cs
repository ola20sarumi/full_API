using full.API.Models;
using Microsoft.EntityFrameworkCore;

namespace full.API.Data;

public class FullDbContext(DbContextOptions<FullDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(employee =>
        {
            employee.Property(e => e.Name).HasMaxLength(EmployeeLimits.NameMaxLength);
            employee.Property(e => e.Email).HasMaxLength(EmployeeLimits.EmailMaxLength);
            employee.Property(e => e.Phone).HasMaxLength(EmployeeLimits.PhoneMaxLength);
            employee.Property(e => e.Department).HasMaxLength(EmployeeLimits.DepartmentMaxLength);
            employee.Property(e => e.Salary).HasPrecision(18, 2);

            employee.HasIndex(e => e.Email).IsUnique();
        });
    }
}
