namespace PortfolioApi.Dtos;

public record BlogpostResponse(
    int Id,
    string Titel,
    string Inhoud,
    DateTime Publicatiedatum);