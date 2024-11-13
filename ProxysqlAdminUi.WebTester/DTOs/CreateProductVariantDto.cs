using System.ComponentModel.DataAnnotations;

namespace ProxysqlAdminUi.WebTester.DTOs;


public class CreateProductVariantDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    public decimal PriceModifier { get; set; }

    public int Stock { get; set; }

    [StringLength(200)]
    public string SKU { get; set; }
}
