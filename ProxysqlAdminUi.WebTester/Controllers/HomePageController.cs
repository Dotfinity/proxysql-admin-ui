using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProxysqlAdminUi.WebTester.Data;
using ProxysqlAdminUi.WebTester.DTOs;

namespace ProxysqlAdminUi.WebTester.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HomePageController : ControllerBase
{
    private readonly EcommerceDbContext _context;

    public HomePageController(EcommerceDbContext context)
    {
        _context = context;
    }

    // GET: api/homepage
    [HttpGet]
    public async Task<ActionResult<HomePageResponseDto>> GetHomePage()
    {
        var homePage = await _context.HomePages
            .Include(h => h.FeaturedProducts)
            .ThenInclude(fp => fp.Product)
            .ThenInclude(p => p.Images)
            .Include(h => h.FeaturedCategories)
            .ThenInclude(fc => fc.Category)
            .ThenInclude(c => c.Products)
            .FirstOrDefaultAsync();

        if (homePage == null)
        {
            return NotFound("Homepage content not found. Please initialize the homepage.");
        }

        var response = new HomePageResponseDto
        {
            Id = homePage.Id,
            HeroTitle = homePage.HeroTitle,
            HeroSubtitle = homePage.HeroSubtitle,
            HeroImageUrl = homePage.HeroImageUrl,
            HeroButtonText = homePage.HeroButtonText,
            HeroButtonUrl = homePage.HeroButtonUrl,
            FeaturedProducts = homePage.FeaturedProducts
                .OrderBy(fp => fp.DisplayOrder)
                .Select(fp => new FeaturedProductResponseDto
                {
                    Id = fp.Id,
                    ProductId = fp.ProductId,
                    ProductName = fp.Product.Name,
                    ProductImageUrl = fp.Product.Images
                        .FirstOrDefault(i => i.IsMain)?.Url ?? "",
                    ProductPrice = fp.Product.Price,
                    DisplayOrder = fp.DisplayOrder
                }).ToList(),
            FeaturedCategories = homePage.FeaturedCategories
                .OrderBy(fc => fc.DisplayOrder)
                .Select(fc => new FeaturedCategoryResponseDto
                {
                    Id = fc.Id,
                    CategoryId = fc.CategoryId,
                    CategoryName = fc.Category.Name,
                    CategorySlug = fc.Category.Slug,
                    ProductCount = fc.Category.Products.Count,
                    DisplayOrder = fc.DisplayOrder
                }).ToList()
        };

        return response;
    }

    // PUT: api/homepage
    [HttpPut]
    public async Task<IActionResult> UpdateHomePage(UpdateHomePageDto dto)
    {
        var homePage = await _context.HomePages.FirstOrDefaultAsync();

        if (homePage == null)
        {
            homePage = new HomePage
            {
                HeroTitle = dto.HeroTitle,
                HeroSubtitle = dto.HeroSubtitle,
                HeroImageUrl = dto.HeroImageUrl,
                HeroButtonText = dto.HeroButtonText,
                HeroButtonUrl = dto.HeroButtonUrl
            };
            _context.HomePages.Add(homePage);
        }
        else
        {
            homePage.HeroTitle = dto.HeroTitle;
            homePage.HeroSubtitle = dto.HeroSubtitle;
            homePage.HeroImageUrl = dto.HeroImageUrl;
            homePage.HeroButtonText = dto.HeroButtonText;
            homePage.HeroButtonUrl = dto.HeroButtonUrl;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // PUT: api/homepage/featured-products
    [HttpPut("featured-products")]
    public async Task<IActionResult> UpdateFeaturedProducts(List<FeaturedItemDto> featuredProducts)
    {
        var homePage = await _context.HomePages
            .Include(h => h.FeaturedProducts)
            .FirstOrDefaultAsync();

        if (homePage == null)
        {
            return NotFound("Homepage not found");
        }

        // Validate products exist
        var productIds = featuredProducts.Select(fp => fp.Id).ToList();
        var existingProducts = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync();

        if (existingProducts.Count != productIds.Count)
        {
            return BadRequest("One or more product IDs are invalid");
        }

        // Remove existing featured products
        _context.FeaturedProducts.RemoveRange(homePage.FeaturedProducts);

        // Add new featured products
        homePage.FeaturedProducts = featuredProducts.Select(fp => new FeaturedProduct
        {
            ProductId = fp.Id,
            DisplayOrder = fp.DisplayOrder,
            HomePageId = homePage.Id
        }).ToList();

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // PUT: api/homepage/featured-categories
    [HttpPut("featured-categories")]
    public async Task<IActionResult> UpdateFeaturedCategories(List<FeaturedItemDto> featuredCategories)
    {
        var homePage = await _context.HomePages
            .Include(h => h.FeaturedCategories)
            .FirstOrDefaultAsync();

        if (homePage == null)
        {
            return NotFound("Homepage not found");
        }

        // Validate categories exist
        var categoryIds = featuredCategories.Select(fc => fc.Id).ToList();
        var existingCategories = await _context.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync();

        if (existingCategories.Count != categoryIds.Count)
        {
            return BadRequest("One or more category IDs are invalid");
        }

        // Remove existing featured categories
        _context.FeaturedCategories.RemoveRange(homePage.FeaturedCategories);

        // Add new featured categories
        homePage.FeaturedCategories = featuredCategories.Select(fc => new FeaturedCategory
        {
            CategoryId = fc.Id,
            DisplayOrder = fc.DisplayOrder,
            HomePageId = homePage.Id
        }).ToList();

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/homepage/featured-products/{id}
    [HttpDelete("featured-products/{id}")]
    public async Task<IActionResult> RemoveFeaturedProduct(int id)
    {
        var featuredProduct = await _context.FeaturedProducts.FindAsync(id);
        if (featuredProduct == null)
        {
            return NotFound();
        }

        _context.FeaturedProducts.Remove(featuredProduct);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/homepage/featured-categories/{id}
    [HttpDelete("featured-categories/{id}")]
    public async Task<IActionResult> RemoveFeaturedCategory(int id)
    {
        var featuredCategory = await _context.FeaturedCategories.FindAsync(id);
        if (featuredCategory == null)
        {
            return NotFound();
        }

        _context.FeaturedCategories.Remove(featuredCategory);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PATCH: api/homepage/hero-image
    [HttpPatch("hero-image")]
    public async Task<IActionResult> UpdateHeroImage([FromBody, Required][Url] string imageUrl)
    {
        var homePage = await _context.HomePages.FirstOrDefaultAsync();
        if (homePage == null)
        {
            return NotFound("Homepage not found");
        }

        homePage.HeroImageUrl = imageUrl;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PATCH: api/homepage/hero-button
    [HttpPatch("hero-button")]
    public async Task<IActionResult> UpdateHeroButton(
        [FromBody] dynamic dto)
    {
        string text = dto.text;
        string url = dto.url;

        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(url))
        {
            return BadRequest("Button text and URL are required");
        }

        var homePage = await _context.HomePages.FirstOrDefaultAsync();
        if (homePage == null)
        {
            return NotFound("Homepage not found");
        }

        homePage.HeroButtonText = text;
        homePage.HeroButtonUrl = url;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}