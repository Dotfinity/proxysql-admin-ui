using System.ComponentModel.DataAnnotations;

namespace ProxysqlAdminUi.WebTester.DTOs;

public class UpdateShopSettingsDto
{
    [StringLength(200)]
    public string ShopName { get; set; }

    [StringLength(500)]
    public string ShopDescription { get; set; }

    [EmailAddress]
    [StringLength(200)]
    public string ContactEmail { get; set; }

    [Phone]
    [StringLength(50)]
    public string ContactPhone { get; set; }

    [StringLength(200)]
    public string Currency { get; set; }

    public bool? EnableProductReviews { get; set; }

    [Range(0, int.MaxValue)]
    public int? MinimumOrderAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? ShippingThreshold { get; set; }
}