namespace ProxysqlAdminUi.WebTester.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Slug { get; set; }
    public int? ParentCategoryId { get; set; }
    public string ParentCategoryName { get; set; }
    public int ProductCount { get; set; }
    public List<CategoryDto> Subcategories { get; set; }
}