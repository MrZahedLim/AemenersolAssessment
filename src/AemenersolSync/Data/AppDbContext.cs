using AemenersolSync.Models;
using Microsoft.EntityFrameworkCore;

namespace AemenersolSync.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Platform> Platforms => Set<Platform>();
    public DbSet<Well> Wells => Set<Well>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Platform>(entity =>
        {
            entity.ToTable("Platform");
            entity.HasKey(p => p.Id);
            // Id datang dari API, jangan biar SQL generate
            entity.Property(p => p.Id).ValueGeneratedNever();
            entity.Property(p => p.UniqueName).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Latitude).HasColumnType("decimal(12,6)");
            entity.Property(p => p.Longitude).HasColumnType("decimal(12,6)");
            entity.Property(p => p.CreatedAt).HasColumnType("datetime2");
            entity.Property(p => p.UpdatedAt).HasColumnType("datetime2");

            entity.HasMany(p => p.Wells)
                  .WithOne(w => w.Platform)
                  .HasForeignKey(w => w.PlatformId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Well>(entity =>
        {
            entity.ToTable("Well");
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Id).ValueGeneratedNever();
            entity.Property(w => w.UniqueName).HasMaxLength(200).IsRequired();
            entity.Property(w => w.Latitude).HasColumnType("decimal(12,6)");
            entity.Property(w => w.Longitude).HasColumnType("decimal(12,6)");
            entity.Property(w => w.CreatedAt).HasColumnType("datetime2");
            entity.Property(w => w.UpdatedAt).HasColumnType("datetime2");
        });
    }
}
