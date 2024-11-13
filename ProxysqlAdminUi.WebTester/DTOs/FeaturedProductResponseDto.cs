namespace ProxysqlAdminUi.WebTester.DTOs;

public class FeaturedProductResponseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string ProductImageUrl { get; set; }
    public decimal ProductPrice { get; set; }
    public int DisplayOrder { get; set; }
}