using System.ComponentModel.DataAnnotations;

namespace PortfolioApi.Dtos;

public class CreateProjectRequest
{
    [Required]
    [StringLength(100)]
    public string Titel { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Beschrijving { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Categorie { get; set; } = string.Empty;

    [Url]
    public string? GitHubUrl { get; set; }

    public DateTime Datum { get; set; }
}