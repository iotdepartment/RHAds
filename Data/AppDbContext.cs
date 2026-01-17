using Microsoft.EntityFrameworkCore;
using RHAds.Models.Safety;
using RHAds.Models.Areas;

namespace RHAds.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Tablas globales
        public DbSet<Area> Areas { get; set; }
        public DbSet<Slide> Slides { get; set; }
        public DbSet<SlideImage> SlideImages { get; set; }
        public DbSet<SlideLayout> SlideLayouts { get; set; } // ← FALTABA ESTO
        public DbSet<SafetyEvent> SafetyEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relaciones
            modelBuilder.Entity<Area>()
                .HasMany(a => a.Slides)
                .WithOne(s => s.Area)
                .HasForeignKey(s => s.AreaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Slide>()
                .HasMany(s => s.SlideImages)
                .WithOne(i => i.Slide)
                .HasForeignKey(i => i.SlideId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SlideImage>()
                .HasKey(i => i.ImageId);

            modelBuilder.Entity<SlideLayout>()
                .HasOne(sl => sl.Slide)
                .WithMany()
                .HasForeignKey(sl => sl.SlideId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SlideLayout>()
                .HasOne(sl => sl.Area)
                .WithMany(a => a.SlideLayouts)
                .HasForeignKey(sl => sl.AreaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}