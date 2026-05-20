using Microsoft.EntityFrameworkCore;
using TelRad.Models;

namespace TelRad.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<Employee> Employees { get; set; } = null!;

        public DbSet<Telrad> Telrads { get; set; } = null!;
        public DbSet<AdminCredentials> Admins { get; set; } = null!;
    }
}