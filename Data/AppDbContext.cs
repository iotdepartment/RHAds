using Microsoft.EntityFrameworkCore;
using RHAds.Models.Areas;
using RHAds.Models.Safety;
using RHAds.Models.Usuarios;

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
        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<SlideImage> SlideImages { get; set; }
        public DbSet<SlideLayout> SlideLayouts { get; set; }
        public DbSet<SafetyEvent> SafetyEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relación Area → Slides (un área tiene muchos slides)
            modelBuilder.Entity<Area>()
                .HasMany(a => a.Slides)
                .WithOne(s => s.Area)
                .HasForeignKey(s => s.AreaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Usuario → Area (un usuario pertenece a un área)
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Area)
                .WithMany(a => a.Usuarios)
                .HasForeignKey(u => u.AreaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Slide → SlideImages (un slide tiene muchas imágenes)
            modelBuilder.Entity<Slide>()
                .HasMany(s => s.SlideImages)
                .WithOne(i => i.Slide)
                .HasForeignKey(i => i.SlideId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Slide → AreaDestino (slide global apunta a un área destino)
            modelBuilder.Entity<Slide>()
                .HasOne(s => s.AreaDestino)
                .WithMany()
                .HasForeignKey(s => s.AreaDestinoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Clave primaria SlideImage
            modelBuilder.Entity<SlideImage>()
                .HasKey(i => i.ImageId);

            // Relación SlideLayout → Slide (cascade delete)
            modelBuilder.Entity<SlideLayout>()
                .HasOne(sl => sl.Slide)
                .WithMany(s => s.SlideLayouts)
                .HasForeignKey(sl => sl.SlideId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación SlideLayout → Area (restrict, porque no quieres borrar layouts al borrar área destino)
            modelBuilder.Entity<SlideLayout>()
                .HasOne(sl => sl.Area)
                .WithMany(a => a.SlideLayouts)
                .HasForeignKey(sl => sl.AreaId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Relación Area → SafetyEvents (cascade delete)
            modelBuilder.Entity<Area>()
                .HasMany(a => a.SafetyEvents)
                .WithOne(se => se.Area)
                .HasForeignKey(se => se.AreaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}