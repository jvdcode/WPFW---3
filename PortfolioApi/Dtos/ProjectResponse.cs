namespace PortfolioApi.Dtos;

public record ProjectResponse(
    int Id,
    string Titel,
    string Beschrijving,
    string Categorie,
    string? GitHubUrl,
    DateTime Datum);