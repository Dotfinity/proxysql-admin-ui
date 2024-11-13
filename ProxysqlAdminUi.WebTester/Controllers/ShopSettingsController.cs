using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProxysqlAdminUi.WebTester.Data;
using ProxysqlAdminUi.WebTester.DTOs;

namespace ProxysqlAdminUi.WebTester.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShopSettingsController : ControllerBase
{
    private readonly EcommerceDbContext _context;

    public ShopSettingsController(EcommerceDbContext context)
    {
        _context = context;
    }

    // GET: api/shopsettings
    [HttpGet]
    public async Task<ActionResult<ShopSettingsResponseDto>> GetShopSettings()
    {
        var settings = await _context.ShopSettings
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (settings == null)
        {
            return NotFound("Shop settings not found. Please initialize the settings.");
        }

        var response = new ShopSettingsResponseDto
        {
            Id = settings.Id,
            ShopName = settings.ShopName,
            ShopDescription = settings.ShopDescription,
            ContactEmail = settings.ContactEmail,
            ContactPhone = settings.ContactPhone,
            Currency = settings.Currency,
            EnableProductReviews = settings.EnableProductReviews,
            MinimumOrderAmount = settings.MinimumOrderAmount,
            ShippingThreshold = settings.ShippingThreshold,
            UpdatedAt = settings.UpdatedAt
        };

        return response;
    }

    // PUT: api/shopsettings
    [HttpPut]
    public async Task<IActionResult> UpdateShopSettings(UpdateShopSettingsDto dto)
    {
        var settings = await _context.ShopSettings.FirstOrDefaultAsync();

        if (settings == null)
        {
            settings = new ShopSettings
            {
                Id = 1,
                ShopName = "Default Shop Name",
                Currency = "USD",
                EnableProductReviews = true,
                UpdatedAt = DateTime.UtcNow
            };
            _context.ShopSettings.Add(settings);
        }

        // Update only provided properties
        if (!string.IsNullOrEmpty(dto.ShopName))
            settings.ShopName = dto.ShopName;

        if (!string.IsNullOrEmpty(dto.ShopDescription))
            settings.ShopDescription = dto.ShopDescription;

        if (!string.IsNullOrEmpty(dto.ContactEmail))
            settings.ContactEmail = dto.ContactEmail;

        if (!string.IsNullOrEmpty(dto.ContactPhone))
            settings.ContactPhone = dto.ContactPhone;

        if (!string.IsNullOrEmpty(dto.Currency))
            settings.Currency = dto.Currency;

        if (dto.EnableProductReviews.HasValue)
            settings.EnableProductReviews = dto.EnableProductReviews.Value;

        if (dto.MinimumOrderAmount.HasValue)
            settings.MinimumOrderAmount = dto.MinimumOrderAmount.Value;

        if (dto.ShippingThreshold.HasValue)
            settings.ShippingThreshold = dto.ShippingThreshold.Value;

        settings.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PATCH: api/shopsettings/currency
    [HttpPatch("currency")]
    public async Task<IActionResult> UpdateCurrency([FromBody, Required] string currency)
    {
        if (string.IsNullOrEmpty(currency))
        {
            return BadRequest("Currency cannot be empty");
        }

        var settings = await _context.ShopSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            return NotFound("Shop settings not found");
        }

        settings.Currency = currency;
        settings.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PATCH: api/shopsettings/toggle-reviews
    [HttpPatch("toggle-reviews")]
    public async Task<IActionResult> ToggleProductReviews()
    {
        var settings = await _context.ShopSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            return NotFound("Shop settings not found");
        }

        settings.EnableProductReviews = !settings.EnableProductReviews;
        settings.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PATCH: api/shopsettings/shipping-threshold
    [HttpPatch("shipping-threshold")]
    public async Task<IActionResult> UpdateShippingThreshold([FromBody] decimal threshold)
    {
        if (threshold < 0)
        {
            return BadRequest("Shipping threshold cannot be negative");
        }

        var settings = await _context.ShopSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            return NotFound("Shop settings not found");
        }

        settings.ShippingThreshold = threshold;
        settings.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PATCH: api/shopsettings/minimum-order
    [HttpPatch("minimum-order")]
    public async Task<IActionResult> UpdateMinimumOrder([FromBody] int amount)
    {
        if (amount < 0)
        {
            return BadRequest("Minimum order amount cannot be negative");
        }

        var settings = await _context.ShopSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            return NotFound("Shop settings not found");
        }

        settings.MinimumOrderAmount = amount;
        settings.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}