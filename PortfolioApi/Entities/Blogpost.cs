namespace PortfolioApi.Entities;

public class Blogpost
{
    public int Id { get; set; }

    public required string Titel { get; set; }

    public required string Inhoud { get; set; }

    public DateTime Publicatiedatum { get; set; }
}