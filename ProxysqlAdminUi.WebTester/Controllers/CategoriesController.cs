using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProxysqlAdminUi.WebTester.Data;
using ProxysqlAdminUi.WebTester.DTOs;

namespace ProxysqlAdminUi.WebTester.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly EcommerceDbContext _context;

    public CategoriesController(EcommerceDbContext context)
    {
        _context = context;
    }

    // GET: api/categories
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories(
        [FromQuery] bool includeSubcategories = false,
        [FromQuery] bool topLevelOnly = false,
        [FromQuery] string search = null)
    {
        IQueryable<Category> query = _context.Categories
            .Include(c => c.Products)
            .Include(c => c.ParentCategory);

        if (includeSubcategories)
        {
            query = query.Include(c => c.Subcategories);
        }

        if (topLevelOnly)
        {
            query = query.Where(c => c.ParentCategoryId == null);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c =>
                c.Name.Contains(search) ||
                c.Description.Contains(search));
        }

        var categories = await query.ToListAsync();

        var categoryDtos = categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Slug = c.Slug,
            ParentCategoryId = c.ParentCategoryId,
            ParentCategoryName = c.ParentCategory?.Name,
            ProductCount = c.Products.Count,
            Subcategories = includeSubcategories
                ? c.Subcategories?.Select(sc => new CategoryDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    Description = sc.Description,
                    Slug = sc.Slug,
                    ParentCategoryId = sc.ParentCategoryId,
                    ProductCount = sc.Products.Count
                }).ToList()
                : null
        }).ToList();

        return Ok(categoryDtos);
    }

    // GET: api/categories/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .Include(c => c.ParentCategory)
            .Include(c => c.Subcategories)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        var categoryDto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Slug = category.Slug,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = category.ParentCategory?.Name,
            ProductCount = category.Products.Count,
            Subcategories = category.Subcategories?.Select(sc => new CategoryDto
            {
                Id = sc.Id,
                Name = sc.Name,
                Description = sc.Description,
                Slug = sc.Slug,
                ParentCategoryId = sc.ParentCategoryId,
                ProductCount = sc.Products.Count
            }).ToList()
        };

        return categoryDto;
    }

    // GET: api/categories/{slug}
    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<CategoryDto>> GetCategoryBySlug(string slug)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .Include(c => c.ParentCategory)
            .Include(c => c.Subcategories)
            .FirstOrDefaultAsync(c => c.Slug == slug);

        if (category == null)
        {
            return NotFound();
        }

        var categoryDto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Slug = category.Slug,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = category.ParentCategory?.Name,
            ProductCount = category.Products.Count,
            Subcategories = category.Subcategories?.Select(sc => new CategoryDto
            {
                Id = sc.Id,
                Name = sc.Name,
                Description = sc.Description,
                Slug = sc.Slug,
                ParentCategoryId = sc.ParentCategoryId,
                ProductCount = sc.Products.Count
            }).ToList()
        };

        return categoryDto;
    }

    // POST: api/categories
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto dto)
    {
        // Validate parent category if provided
        if (dto.ParentCategoryId.HasValue)
        {
            var parentExists = await _context.Categories
                .AnyAsync(c => c.Id == dto.ParentCategoryId.Value);
            if (!parentExists)
            {
                return BadRequest("Invalid parent category ID");
            }
        }

        // Generate slug if not provided
        var slug = !string.IsNullOrEmpty(dto.Slug)
            ? dto.Slug
            : GenerateSlug(dto.Name);

        // Validate slug uniqueness
        var slugExists = await _context.Categories
            .AnyAsync(c => c.Slug == slug);
        if (slugExists)
        {
            return BadRequest("Category with this slug already exists");
        }

        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            Slug = slug,
            ParentCategoryId = dto.ParentCategoryId
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetCategory),
            new { id = category.Id },
            await GetCategory(category.Id));
    }

    // PUT: api/categories/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryDto dto)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        // Validate parent category if provided
        if (dto.ParentCategoryId.HasValue)
        {
            // Prevent circular reference
            if (dto.ParentCategoryId.Value == id)
            {
                return BadRequest("Category cannot be its own parent");
            }

            // Check if new parent exists
            var parentExists = await _context.Categories
                .AnyAsync(c => c.Id == dto.ParentCategoryId.Value);
            if (!parentExists)
            {
                return BadRequest("Invalid parent category ID");
            }

            // Prevent making a category a child of its own descendant
            var descendants = await GetDescendantIds(id);
            if (descendants.Contains(dto.ParentCategoryId.Value))
            {
                return BadRequest("Cannot make a category a child of its own descendant");
            }

            category.ParentCategoryId = dto.ParentCategoryId;
        }

        // Update slug if provided or if name is changed
        if (!string.IsNullOrEmpty(dto.Slug) || !string.IsNullOrEmpty(dto.Name))
        {
            var newSlug = !string.IsNullOrEmpty(dto.Slug)
                ? dto.Slug
                : GenerateSlug(dto.Name ?? category.Name);

            if (newSlug != category.Slug)
            {
                var slugExists = await _context.Categories
                    .AnyAsync(c => c.Slug == newSlug && c.Id != id);
                if (slugExists)
                {
                    return BadRequest("Category with this slug already exists");
                }
                category.Slug = newSlug;
            }
        }

        // Update other properties if provided
        if (!string.IsNullOrEmpty(dto.Name))
            category.Name = dto.Name;

        if (!string.IsNullOrEmpty(dto.Description))
            category.Description = dto.Description;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/categories/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .Include(c => c.Subcategories)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        // Check if category has products
        if (category.Products.Any())
        {
            return BadRequest("Cannot delete category with existing products");
        }

        // Check if category has subcategories
        if (category.Subcategories.Any())
        {
            return BadRequest("Cannot delete category with existing subcategories");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Helper methods
    private string GenerateSlug(string name)
    {
        // Convert to lowercase and remove special characters
        var slug = name.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

        // Convert spaces to dashes
        slug = Regex.Replace(slug, @"\s+", "-");

        // Remove multiple dashes
        slug = Regex.Replace(slug, @"-+", "-");

        // Trim dashes from ends
        return slug.Trim('-');
    }

    private async Task<HashSet<int>> GetDescendantIds(int categoryId)
    {
        var descendants = new HashSet<int>();
        var subcategories = await _context.Categories
            .Where(c => c.ParentCategoryId == categoryId)
            .Select(c => c.Id)
            .ToListAsync();

        foreach (var subcategoryId in subcategories)
        {
            descendants.Add(subcategoryId);
            var subDescendants = await GetDescendantIds(subcategoryId);
            descendants.UnionWith(subDescendants);
        }

        return descendants;
    }
}