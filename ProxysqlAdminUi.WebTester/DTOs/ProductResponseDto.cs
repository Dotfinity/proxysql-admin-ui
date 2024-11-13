namespace ProxysqlAdminUi.WebTester.DTOs;


public class ProductResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public int Stock { get; set; }
    public string SKU { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CategoryName { get; set; }
    public string MainImageUrl { get; set; }
    public List<ProductVariantDto> Variants { get; set; }
}
