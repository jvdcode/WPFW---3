using System.ComponentModel.DataAnnotations;

namespace PortfolioApi.Dtos;

public class CreateBlogpostRequest
{
    [Required]
    [StringLength(100)]
    public string Titel { get; set; } = string.Empty;

    [Required]
    [StringLength(5000)]
    public string Inhoud { get; set; } = string.Empty;

    public DateTime Publicatiedatum { get; set; }
}