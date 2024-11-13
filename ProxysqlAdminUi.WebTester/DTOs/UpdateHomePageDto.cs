using System.ComponentModel.DataAnnotations;

namespace ProxysqlAdminUi.WebTester.DTOs;

public class UpdateHomePageDto
{
    [Required]
    [StringLength(200)]
    public string HeroTitle { get; set; }

    [StringLength(500)]
    public string HeroSubtitle { get; set; }

    [StringLength(500)]
    [Url]
    public string HeroImageUrl { get; set; }

    [StringLength(200)]
    public string HeroButtonText { get; set; }

    [StringLength(500)]
    [Url]
    public string HeroButtonUrl { get; set; }
}