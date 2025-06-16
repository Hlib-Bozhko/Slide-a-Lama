using Microsoft.EntityFrameworkCore;
using Datas.Entity;

namespace Datas.Service
{
    public class SlideALamaDbContext : DbContext
    {
        public SlideALamaDbContext(DbContextOptions<SlideALamaDbContext> options) 
            : base(options)
        {
        }

        public DbSet<Score> Scores { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Rating> Rating { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Score>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.Property(e => e.Player)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.Points)
                    .IsRequired();
                      
                entity.HasIndex(e => e.Points)
                    .HasDatabaseName("IX_Score_Points");
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasMaxLength(1000);
            });

            modelBuilder.Entity<Rating>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.mark)
                    .IsRequired()
                    .HasAnnotation("Range", new[] { 0, 100 });
            });
        }
    }
}

