namespace PortfolioApi.Entities;

public class Project
{
    public int Id { get; set; }

    public required string Titel { get; set; }

    public required string Beschrijving { get; set; }

    public required string Categorie { get; set; }

    public string? GitHubUrl { get; set; }

    public DateTime Datum { get; set; }
}