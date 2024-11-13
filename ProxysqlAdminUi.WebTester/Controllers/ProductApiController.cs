using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProxysqlAdminUi.WebTester.Data;
using ProxysqlAdminUi.WebTester.DTOs;

namespace ProxysqlAdminUi.WebTester.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly EcommerceDbContext _context;

    public ProductsController(EcommerceDbContext context)
    {
        _context = context;
    }

    // GET: api/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string search = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] bool includeInactive = false)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.Name.Contains(search) ||
                p.Description.Contains(search) ||
                p.SKU.Contains(search));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        var totalItems = await query.CountAsync();

        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                IsActive = p.IsActive,
                Stock = p.Stock,
                SKU = p.SKU,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                CategoryName = p.Category.Name,
                MainImageUrl = p.Images.FirstOrDefault(i => i.IsMain).Url,
                Variants = p.Variants.Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    PriceModifier = v.PriceModifier,
                    Stock = v.Stock,
                    SKU = v.SKU
                }).ToList()
            })
            .ToListAsync();

        Response.Headers.Add("X-Total-Count", totalItems.ToString());
        Response.Headers.Add("X-Total-Pages", Math.Ceiling((double)totalItems / pageSize).ToString());

        return Ok(products);
    }

    // GET: api/products/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponseDto>> GetProduct(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        var response = new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            IsActive = product.IsActive,
            Stock = product.Stock,
            SKU = product.SKU,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            CategoryName = product.Category.Name,
            MainImageUrl = product.Images.FirstOrDefault(i => i.IsMain)?.Url,
            Variants = product.Variants.Select(v => new ProductVariantDto
            {
                Id = v.Id,
                Name = v.Name,
                PriceModifier = v.PriceModifier,
                Stock = v.Stock,
                SKU = v.SKU
            }).ToList()
        };

        return response;
    }

    // POST: api/products
    [HttpPost]
    public async Task<ActionResult<ProductResponseDto>> CreateProduct(CreateProductDto dto)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
        {
            return BadRequest("Invalid category ID");
        }

        if (!string.IsNullOrEmpty(dto.SKU))
        {
            var skuExists = await _context.Products.AnyAsync(p => p.SKU == dto.SKU);
            if (skuExists)
            {
                return BadRequest("SKU already exists");
            }
        }

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            SKU = dto.SKU,
            CategoryId = dto.CategoryId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.Variants != null)
        {
            product.Variants = dto.Variants.Select(v => new ProductVariant
            {
                Name = v.Name,
                PriceModifier = v.PriceModifier,
                Stock = v.Stock,
                SKU = v.SKU
            }).ToList();
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, await GetProduct(product.Id));
    }

    // PUT: api/products/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        if (dto.CategoryId.HasValue)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId.Value);
            if (!categoryExists)
            {
                return BadRequest("Invalid category ID");
            }
            product.CategoryId = dto.CategoryId.Value;
        }

        if (!string.IsNullOrEmpty(dto.SKU) && dto.SKU != product.SKU)
        {
            var skuExists = await _context.Products.AnyAsync(p => p.SKU == dto.SKU);
            if (skuExists)
            {
                return BadRequest("SKU already exists");
            }
            product.SKU = dto.SKU;
        }

        if (!string.IsNullOrEmpty(dto.Name))
            product.Name = dto.Name;

        if (!string.IsNullOrEmpty(dto.Description))
            product.Description = dto.Description;

        if (dto.Price.HasValue)
            product.Price = dto.Price.Value;

        if (dto.Stock.HasValue)
            product.Stock = dto.Stock.Value;

        if (dto.IsActive.HasValue)
            product.IsActive = dto.IsActive.Value;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/products/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PATCH: api/products/5/stock
    [HttpPatch("{id}/stock")]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] int stock)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        if (stock < 0)
        {
            return BadRequest("Stock cannot be negative");
        }

        product.Stock = stock;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PATCH: api/products/5/toggle-status
    [HttpPatch("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        product.IsActive = !product.IsActive;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}