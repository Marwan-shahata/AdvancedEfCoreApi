using AdvancedEFCoreAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AdvancedEFCoreAPI.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(c => c.Name).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.IsDeleted).HasDefaultValue(false);
            entity.Property(p => p.RowVersion).IsRowVersion();

            entity.HasIndex(p => p.Name);
            entity.HasQueryFilter(p => !p.IsDeleted);
            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
