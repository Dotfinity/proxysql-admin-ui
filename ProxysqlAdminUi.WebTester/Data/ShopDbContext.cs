using Microsoft.EntityFrameworkCore;

namespace ProxysqlAdminUi.WebTester.Data;

public class EcommerceDbContext : DbContext
{
    public EcommerceDbContext(DbContextOptions<EcommerceDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ShopSettings> ShopSettings { get; set; }
    public DbSet<HomePage> HomePages { get; set; }
    public DbSet<FeaturedProduct> FeaturedProducts { get; set; }
    public DbSet<FeaturedCategory> FeaturedCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.SKU)
                .IsUnique()
                .HasFilter("[SKU] IS NOT NULL");

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Images)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Variants)
                .WithOne(v => v.Product)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProductVariant configuration
        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasIndex(e => e.SKU)
                .IsUnique()
                .HasFilter("[SKU] IS NOT NULL");
        });

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.Slug)
                .IsUnique()
                .HasFilter("[Slug] IS NOT NULL");

            entity.HasOne(e => e.ParentCategory)
                .WithMany(c => c.Subcategories)
                .HasForeignKey(e => e.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ShopSettings configuration
        modelBuilder.Entity<ShopSettings>(entity =>
        {
            // Ensure only one shop settings record exists
            entity.HasData(new ShopSettings
            {
                Id = 1,
                ShopName = "Default Shop Name",
                Currency = "USD",
                EnableProductReviews = true,
                UpdatedAt = DateTime.UtcNow
            });
        });

        // HomePage configuration
        modelBuilder.Entity<HomePage>(entity =>
        {
            entity.HasMany(e => e.FeaturedProducts)
                .WithOne(fp => fp.HomePage)
                .HasForeignKey(fp => fp.HomePageId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.FeaturedCategories)
                .WithOne(fc => fc.HomePage)
                .HasForeignKey(fc => fc.HomePageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // FeaturedProduct configuration
        modelBuilder.Entity<FeaturedProduct>(entity =>
        {
            entity.HasIndex(e => new { e.HomePageId, e.ProductId })
                .IsUnique();

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // FeaturedCategory configuration
        modelBuilder.Entity<FeaturedCategory>(entity =>
        {
            entity.HasIndex(e => new { e.HomePageId, e.CategoryId })
                .IsUnique();

            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatically update timestamps before saving
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is Product && e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Product product)
            {
                product.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
