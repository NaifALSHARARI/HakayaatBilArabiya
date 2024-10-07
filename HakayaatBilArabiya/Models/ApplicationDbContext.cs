using Microsoft.EntityFrameworkCore;

namespace HakayaatBilArabiya.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Story> Stories { get; set; }
        public DbSet<Comment> Comments { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Story>()
                .HasMany(s => s.Comments)
                .WithOne(c => c.Story)
                .HasForeignKey(c => c.StoryId);
        }
    }
}
