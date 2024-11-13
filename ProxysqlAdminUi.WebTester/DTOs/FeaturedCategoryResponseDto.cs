namespace ProxysqlAdminUi.WebTester.DTOs;

public class FeaturedCategoryResponseDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public string CategorySlug { get; set; }
    public int ProductCount { get; set; }
    public int DisplayOrder { get; set; }
}