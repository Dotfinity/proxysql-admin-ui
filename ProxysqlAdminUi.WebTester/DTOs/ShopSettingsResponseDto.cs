namespace ProxysqlAdminUi.WebTester.DTOs;

public class ShopSettingsResponseDto
{
    public int Id { get; set; }
    public string ShopName { get; set; }
    public string ShopDescription { get; set; }
    public string ContactEmail { get; set; }
    public string ContactPhone { get; set; }
    public string Currency { get; set; }
    public bool EnableProductReviews { get; set; }
    public int MinimumOrderAmount { get; set; }
    public decimal ShippingThreshold { get; set; }
    public DateTime UpdatedAt { get; set; }
}