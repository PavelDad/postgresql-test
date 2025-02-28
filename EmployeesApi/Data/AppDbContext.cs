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
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.HasPostgresExtension("uuid-ossp"); // Включение расширения для UUID

            modelBuilder.Entity<Employee>()
                .Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()"); // Генерация UUID по умолчанию

            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .HasDefaultValueSql("uuid_generate_v4()");

            base.OnModelCreating(modelBuilder);
        }
    }
}