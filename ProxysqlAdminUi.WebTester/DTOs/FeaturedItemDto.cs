using System.ComponentModel.DataAnnotations;

namespace ProxysqlAdminUi.WebTester.DTOs;

public class FeaturedItemDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int DisplayOrder { get; set; }
}