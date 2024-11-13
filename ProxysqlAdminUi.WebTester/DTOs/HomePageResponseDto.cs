namespace ProxysqlAdminUi.WebTester.DTOs;

public class HomePageResponseDto
{
    public int Id { get; set; }
    public string HeroTitle { get; set; }
    public string HeroSubtitle { get; set; }
    public string HeroImageUrl { get; set; }
    public string HeroButtonText { get; set; }
    public string HeroButtonUrl { get; set; }
    public List<FeaturedProductResponseDto> FeaturedProducts { get; set; }
    public List<FeaturedCategoryResponseDto> FeaturedCategories { get; set; }
}