using System.ComponentModel.DataAnnotations;

namespace ProxysqlAdminUi.WebTester.DTOs;

public class UpdateCategoryDto
{
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    [StringLength(100)]
    public string Slug { get; set; }

    public int? ParentCategoryId { get; set; }
}