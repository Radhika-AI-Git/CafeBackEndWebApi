using Microsoft.EntityFrameworkCore;
using CafeBackEndWebApi.Models;
namespace CafeBackEndWebApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext
        (DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
        public DbSet<Models.Employee> Employees { get; set; }
        public DbSet<Models.Cafe> Cafes { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.Id)
                .IsUnique();

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Cafe)
                .WithMany(c => c.Employees)
                .HasForeignKey(e => e.CafeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
