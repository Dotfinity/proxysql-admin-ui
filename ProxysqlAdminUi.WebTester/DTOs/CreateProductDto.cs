using System.ComponentModel.DataAnnotations;

namespace ProxysqlAdminUi.WebTester.DTOs;

 
    public class CreateProductDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public int Stock { get; set; }

        [StringLength(200)]
        public string SKU { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public List<CreateProductVariantDto> Variants { get; set; }
    }
 