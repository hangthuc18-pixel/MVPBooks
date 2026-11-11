
using Microsoft.EntityFrameworkCore;
using MVPBooks.Models;

namespace MVPBooks.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Không cho phép trùng mã sách
            modelBuilder.Entity<Book>()
                .HasIndex(b => b.Code)
                .IsUnique();
        }
    }
}
