using System.Net;
using System.Net.Http.Json;
using PortfolioApi.Auth;
using PortfolioApi.Dtos;

namespace PortfolioApi.Tests;

[Collection("Api")]
public class BlogpostsApiTests
{
    private const string Url = "/api/blogposts";
    private const int OnbekendId = 999999;

    private readonly PortfolioApiFactory _factory;

    public BlogpostsApiTests(PortfolioApiFactory factory)
    {
        _factory = factory;
    }

    // ===== Hulpmethodes =====

    // Een geldige blogpost met een unieke titel
    private static CreateBlogpostRequest NieuweBlogpost()
    {
        return new CreateBlogpostRequest
        {
            Titel = $"Testblogpost {Guid.NewGuid():N}",
            Inhoud = "De inhoud van een blogpost voor de tests.",
            Publicatiedatum = new DateTime(2026, 11, 1)
        };
    }

    // Maakt via de API een blogpost aan als Editor
    private async Task<BlogpostResponse> MaakBlogpostAanAlsEditorAsync()
    {
        var editor = await _factory.CreateClientWithRoleAsync(Roles.Editor);
        var response = await editor.PostAsJsonAsync(Url, NieuweBlogpost());
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<BlogpostResponse>())!;
    }

    // ===== GET =====

    [Fact]
    public async Task GetAll_ZonderAanmelding_Geeft200EnBevatBlogpost()
    {
        // Arrange
        var bestaand = await MaakBlogpostAanAlsEditorAsync();
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(Url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var blogposts = await response.Content.ReadFromJsonAsync<List<BlogpostResponse>>();
        Assert.NotNull(blogposts);
        Assert.Contains(blogposts, b => b.Id == bestaand.Id && b.Titel == bestaand.Titel);
    }

    [Fact]
    public async Task GetById_BestaandId_Geeft200EnJuisteGegevens()
    {
        // Arrange
        var bestaand = await MaakBlogpostAanAlsEditorAsync();
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"{Url}/{bestaand.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var blogpost = await response.Content.ReadFromJsonAsync<BlogpostResponse>();
        Assert.NotNull(blogpost);
        Assert.Equal(bestaand.Titel, blogpost.Titel);
        Assert.Equal(bestaand.Inhoud, blogpost.Inhoud);
    }

    [Fact]
    public async Task GetById_OnbekendId_Geeft404()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"{Url}/{OnbekendId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ===== POST =====

    [Fact]
    public async Task Create_AlsEditor_Geeft201EnSlaatBlogpostOp()
    {
        // Arrange
        var editor = await _factory.CreateClientWithRoleAsync(Roles.Editor);
        var nieuw = NieuweBlogpost();

        // Act
        var response = await editor.PostAsJsonAsync(Url, nieuw);

        // Assert: 201 met de juiste gegevens en een Id van de database
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var aangemaakt = await response.Content.ReadFromJsonAsync<BlogpostResponse>();
        Assert.NotNull(aangemaakt);
        Assert.True(aangemaakt.Id > 0);
        Assert.Equal(nieuw.Titel, aangemaakt.Titel);
        Assert.Equal(nieuw.Inhoud, aangemaakt.Inhoud);

        // Assert: de blogpost is echt opgeslagen
        var opgehaald = await _factory.CreateClient()
            .GetFromJsonAsync<BlogpostResponse>($"{Url}/{aangemaakt.Id}");
        Assert.Equal(nieuw.Titel, opgehaald!.Titel);
    }

    [Fact]
    public async Task Create_AlsAdmin_Geeft201()
    {
        // Arrange
        var admin = await _factory.CreateClientWithRoleAsync(Roles.Admin);
        var nieuw = NieuweBlogpost();

        // Act
        var response = await admin.PostAsJsonAsync(Url, nieuw);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var aangemaakt = await response.Content.ReadFromJsonAsync<BlogpostResponse>();
        Assert.Equal(nieuw.Titel, aangemaakt!.Titel);
    }

    [Fact]
    public async Task Create_ZonderAanmelding_Geeft401EnSlaatNietsOp()
    {
        // Arrange
        var client = _factory.CreateClient();
        var nieuw = NieuweBlogpost();

        // Act
        var response = await client.PostAsJsonAsync(Url, nieuw);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var alle = await client.GetFromJsonAsync<List<BlogpostResponse>>(Url);
        Assert.DoesNotContain(alle!, b => b.Titel == nieuw.Titel);
    }

    [Fact]
    public async Task Create_AlsGast_Geeft403EnSlaatNietsOp()
    {
        // Arrange
        var gast = await _factory.CreateClientWithRoleAsync(Roles.Gast);
        var nieuw = NieuweBlogpost();

        // Act
        var response = await gast.PostAsJsonAsync(Url, nieuw);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var alle = await gast.GetFromJsonAsync<List<BlogpostResponse>>(Url);
        Assert.DoesNotContain(alle!, b => b.Titel == nieuw.Titel);
    }

    [Fact]
    public async Task Create_OngeldigeInvoer_Geeft400()
    {
        // Arrange: lege inhoud, terwijl Inhoud [Required] is
        var editor = await _factory.CreateClientWithRoleAsync(Roles.Editor);
        var ongeldig = NieuweBlogpost();
        ongeldig.Inhoud = "";

        // Act
        var response = await editor.PostAsJsonAsync(Url, ongeldig);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ===== PUT =====

    [Fact]
    public async Task Update_AlsEditor_Geeft200EnWijzigtBlogpost()
    {
        // Arrange
        var bestaand = await MaakBlogpostAanAlsEditorAsync();
        var editor = await _factory.CreateClientWithRoleAsync(Roles.Editor);
        var wijziging = new UpdateBlogpostRequest
        {
            Titel = "Gewijzigde blogpost",
            Inhoud = "Gewijzigde inhoud",
            Publicatiedatum = new DateTime(2026, 11, 15)
        };

        // Act
        var response = await editor.PutAsJsonAsync($"{Url}/{bestaand.Id}", wijziging);

        // Assert: 200 met de nieuwe gegevens
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var gewijzigd = await response.Content.ReadFromJsonAsync<BlogpostResponse>();
        Assert.Equal("Gewijzigde blogpost", gewijzigd!.Titel);

        // Assert: de wijziging is echt opgeslagen
        var opgehaald = await _factory.CreateClient()
            .GetFromJsonAsync<BlogpostResponse>($"{Url}/{bestaand.Id}");
        Assert.Equal("Gewijzigde blogpost", opgehaald!.Titel);
        Assert.Equal("Gewijzigde inhoud", opgehaald.Inhoud);
    }

    [Fact]
    public async Task Update_AlsGast_Geeft403EnWijzigtNiets()
    {
        // Arrange
        var bestaand = await MaakBlogpostAanAlsEditorAsync();
        var gast = await _factory.CreateClientWithRoleAsync(Roles.Gast);
        var wijziging = new UpdateBlogpostRequest
        {
            Titel = "Mag niet",
            Inhoud = "Mag niet",
            Publicatiedatum = new DateTime(2026, 11, 15)
        };

        // Act
        var response = await gast.PutAsJsonAsync($"{Url}/{bestaand.Id}", wijziging);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var opgehaald = await gast.GetFromJsonAsync<BlogpostResponse>($"{Url}/{bestaand.Id}");
        Assert.Equal(bestaand.Titel, opgehaald!.Titel);
    }

    [Fact]
    public async Task Update_OnbekendId_Geeft404()
    {
        // Arrange
        var editor = await _factory.CreateClientWithRoleAsync(Roles.Editor);
        var wijziging = new UpdateBlogpostRequest
        {
            Titel = "Bestaat niet",
            Inhoud = "Bestaat niet",
            Publicatiedatum = new DateTime(2026, 11, 15)
        };

        // Act
        var response = await editor.PutAsJsonAsync($"{Url}/{OnbekendId}", wijziging);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ===== DELETE =====

    [Fact]
    public async Task Delete_AlsAdmin_Geeft204EnVerwijdertBlogpost()
    {
        // Arrange
        var bestaand = await MaakBlogpostAanAlsEditorAsync();
        var admin = await _factory.CreateClientWithRoleAsync(Roles.Admin);

        // Act
        var response = await admin.DeleteAsync($"{Url}/{bestaand.Id}");

        // Assert: 204 en de blogpost is weg
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var opgehaald = await admin.GetAsync($"{Url}/{bestaand.Id}");
        Assert.Equal(HttpStatusCode.NotFound, opgehaald.StatusCode);
    }

    [Fact]
    public async Task Delete_ZonderAanmelding_Geeft401EnBlogpostBestaatNog()
    {
        // Arrange
        var bestaand = await MaakBlogpostAanAlsEditorAsync();
        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"{Url}/{bestaand.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var opgehaald = await client.GetAsync($"{Url}/{bestaand.Id}");
        Assert.Equal(HttpStatusCode.OK, opgehaald.StatusCode);
    }

    [Fact]
    public async Task Delete_OnbekendId_Geeft404()
    {
        // Arrange
        var admin = await _factory.CreateClientWithRoleAsync(Roles.Admin);

        // Act
        var response = await admin.DeleteAsync($"{Url}/{OnbekendId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}