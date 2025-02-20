using Microsoft.EntityFrameworkCore;
using EmployeesApi.Models;

namespace EmployeesApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.HasPostgresExtension("uuid-ossp"); // Включение расширения для UUID

            modelBuilder.Entity<Employee>()
                .Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()"); // Генерация UUID по умолчанию

            base.OnModelCreating(modelBuilder);
        }
    }
}