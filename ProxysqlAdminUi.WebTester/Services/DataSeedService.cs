using Microsoft.EntityFrameworkCore;
using ProxysqlAdminUi.WebTester.Data;

namespace ProxysqlAdminUi.WebTester.Services
{
    public static class DatabaseSeederExtension
    {
        public static async Task SeedDatabase(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<EcommerceDbContext>();

                await context.Database.EnsureCreatedAsync();

                if (!await context.Categories.AnyAsync())
                {
                    await SeedData(context);
                }
            }
        }

        private static async Task SeedData(EcommerceDbContext context)
        {
            var random = new Random();

            // Create categories
            var categories = new List<Category>
            {
                new Category { Name = "Electronics", Description = "Electronic devices and gadgets", Slug = "electronics" },
                new Category { Name = "Clothing", Description = "Fashion and apparel", Slug = "clothing" },
                new Category { Name = "Books", Description = "Books and literature", Slug = "books" },
                new Category { Name = "Home & Garden", Description = "Home improvement and garden supplies", Slug = "home-garden" },
                new Category { Name = "Sports", Description = "Sports equipment and accessories", Slug = "sports" }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();

            // Product name components for random generation
            var adjectives = new[] { "Premium", "Ultra", "Deluxe", "Essential", "Professional", "Classic", "Modern", "Advanced", "Smart", "Eco-friendly" };
            var nouns = new[] { "Widget", "Gadget", "Tool", "Device", "Solution", "System", "Kit", "Pack", "Set", "Bundle" };

            // Create products
            var products = new List<Product>();
            for (int i = 0; i < 10; i++)
            {
                var name = $"{adjectives[random.Next(adjectives.Length)]} {nouns[random.Next(nouns.Length)]}";
                var category = categories[random.Next(categories.Count)];

                var product = new Product
                {
                    Name = name,
                    Description = $"This is a detailed description for {name}. It includes all the amazing features and benefits of this product.",
                    Price = (decimal)Math.Round(random.NextDouble() * 1000, 2),
                    Stock = random.Next(0, 100),
                    SKU = $"SKU{i + 1:D5}",
                    IsActive = random.Next(0, 10) < 8, // 80% chance of being active
                    CategoryId = category.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                    Images = new List<ProductImage>
                    {
                        new ProductImage
                        {
                            Url = $"https://picsum.photos/400/300?random={i+1}",
                            AltText = $"Image of {name}",
                            IsMain = true
                        },
                        new ProductImage
                        {
                            Url = $"https://picsum.photos/400/300?random={i+2}",
                            AltText = $"Another image of {name}",
                            IsMain = false
                        }
                    },
                    Variants = new List<ProductVariant>
                    {
                        new ProductVariant
                        {
                            Name = "Standard",
                            PriceModifier = 0,
                            Stock = random.Next(0, 50),
                            SKU = $"SKU{i + 1:D5}-STD"
                        },
                        new ProductVariant
                        {
                            Name = "Premium",
                            PriceModifier = (decimal)Math.Round(random.NextDouble() * 100, 2),
                            Stock = random.Next(0, 30),
                            SKU = $"SKU{i + 1:D5}-PRE"
                        }
                    }
                };

                products.Add(product);
            }

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            // Create shop settings
            var shopSettings = new ShopSettings
            {
                ShopName = "Demo E-commerce Shop",
                ShopDescription = "This is a demo e-commerce shop with sample products and categories",
                ContactEmail = "demo@example.com",
                ContactPhone = "+1234567890",
                Currency = "USD",
                EnableProductReviews = true,
                MinimumOrderAmount = 10,
                ShippingThreshold = 50,
                UpdatedAt = DateTime.UtcNow
            };

            await context.ShopSettings.AddAsync(shopSettings);
            await context.SaveChangesAsync();

            // Create homepage
            var homePage = new HomePage
            {
                HeroTitle = "Welcome to Our Demo Shop",
                HeroSubtitle = "Discover amazing products at great prices",
                HeroImageUrl = "https://picsum.photos/1200/400",
                HeroButtonText = "Shop Now",
                HeroButtonUrl = "/products",
                FeaturedProducts = products
                    .Take(4)
                    .Select((p, index) => new FeaturedProduct
                    {
                        ProductId = p.Id,
                        DisplayOrder = index + 1
                    })
                    .ToList(),
                FeaturedCategories = categories
                    .Take(3)
                    .Select((c, index) => new FeaturedCategory
                    {
                        CategoryId = c.Id,
                        DisplayOrder = index + 1
                    })
                    .ToList()
            };

            await context.HomePages.AddAsync(homePage);
            await context.SaveChangesAsync();
        }
    }
}