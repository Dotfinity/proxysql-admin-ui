using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProxysqlAdminUi.WebTester.Data;

public class Product
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [StringLength(2000)]
    public string Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    public int Stock { get; set; }

    [StringLength(200)]
    public string SKU { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public int CategoryId { get; set; }
    public Category Category { get; set; }

    public List<ProductImage> Images { get; set; }
    public List<ProductVariant> Variants { get; set; }
}

public class ProductImage
{
    public int Id { get; set; }

    [Required]
    [StringLength(500)]
    public string Url { get; set; }

    [StringLength(100)]
    public string AltText { get; set; }

    public bool IsMain { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }
}

public class ProductVariant
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PriceModifier { get; set; }

    public int Stock { get; set; }

    [StringLength(200)]
    public string SKU { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }
}

public class Category
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    [StringLength(100)]
    public string Slug { get; set; }

    public int? ParentCategoryId { get; set; }
    public Category ParentCategory { get; set; }

    public List<Category> Subcategories { get; set; }
    public List<Product> Products { get; set; }
}

public class ShopSettings
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string ShopName { get; set; }

    [StringLength(500)]
    public string ShopDescription { get; set; }

    [StringLength(200)]
    public string ContactEmail { get; set; }

    [StringLength(50)]
    public string ContactPhone { get; set; }

    [StringLength(200)]
    public string Currency { get; set; } = "USD";

    public bool EnableProductReviews { get; set; } = true;

    public int MinimumOrderAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ShippingThreshold { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class HomePage
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string HeroTitle { get; set; }

    [StringLength(500)]
    public string HeroSubtitle { get; set; }

    [StringLength(500)]
    public string HeroImageUrl { get; set; }

    [StringLength(200)]
    public string HeroButtonText { get; set; }

    [StringLength(500)]
    public string HeroButtonUrl { get; set; }

    public List<FeaturedProduct> FeaturedProducts { get; set; }
    public List<FeaturedCategory> FeaturedCategories { get; set; }
}

public class FeaturedProduct
{
    public int Id { get; set; }
    public int DisplayOrder { get; set; }

    public int HomePageId { get; set; }
    public HomePage HomePage { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }
}

public class FeaturedCategory
{
    public int Id { get; set; }
    public int DisplayOrder { get; set; }

    public int HomePageId { get; set; }
    public HomePage HomePage { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; }
}
