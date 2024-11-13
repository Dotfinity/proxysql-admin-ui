using System.ComponentModel.DataAnnotations;

namespace ProxysqlAdminUi.WebTester.DTOs;


public class UpdateProductDto
{
    [StringLength(200)]
    public string Name { get; set; }

    [StringLength(2000)]
    public string Description { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? Price { get; set; }

    public int? Stock { get; set; }

    [StringLength(200)]
    public string SKU { get; set; }

    public int? CategoryId { get; set; }

    public bool? IsActive { get; set; }
}
