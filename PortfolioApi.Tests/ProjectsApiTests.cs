using System.Net;
using System.Net.Http.Json;
using PortfolioApi.Auth;
using PortfolioApi.Dtos;

namespace PortfolioApi.Tests;

[Collection("Api")]
public class ProjectsApiTests
{
    private const string Url = "/api/projects";
    private const int OnbekendId = 999999;

    private readonly PortfolioApiFactory _factory;

    public ProjectsApiTests(PortfolioApiFactory factory)
    {
        _factory = factory;
    }

    // ===== Hulpmethodes =====

    // Een geldig project met een unieke titel, zodat tests elkaar niet in de weg zitten
    private static CreateProjectRequest NieuwProject()
    {
        return new CreateProjectRequest
        {
            Titel = $"Testproject {Guid.NewGuid():N}",
            Beschrijving = "Een project voor de tests.",
            Categorie = "Test",
            GitHubUrl = "https://github.com/jvdcode",
            Datum = new DateTime(2026, 11, 1)
        };
    }

    // Maakt via de API een project aan als Editor (voor tests die een bestaand project nodig hebben)
    private async Task<ProjectResponse> MaakProjectAanAlsEditorAsync()
    {
        var editor = await _factory.CreateClientWithRoleAsync(Roles.Editor);
        var response = await editor.PostAsJsonAsync(Url, NieuwProject());
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ProjectResponse>())!;
    }

    // ===== GET =====

    [Fact]
    public async Task GetAll_ZonderAanmelding_Geeft200EnBevatProject()
    {
        // Arrange
        var bestaand = await MaakProjectAanAlsEditorAsync();
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(Url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var projecten = await response.Content.ReadFromJsonAsync<List<ProjectResponse>>();
        Assert.NotNull(projecten);
        Assert.Contains(projecten, p => p.Id == bestaand.Id && p.Titel == bestaand.Titel);
    }

    [Fact]
    public async Task GetById_BestaandId_Geeft200EnJuisteGegevens()
    {
        // Arrange
        var bestaand = await MaakProjectAanAlsEditorAsync();
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"{Url}/{bestaand.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var project = await response.Content.ReadFromJsonAsync<ProjectResponse>();
        Assert.NotNull(project);
        Assert.Equal(bestaand.Titel, project.Titel);
        Assert.Equal(bestaand.Categorie, project.Categorie);
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
    public async Task Create_AlsEditor_Geeft201EnSlaatProjectOp()
    {
        // Arrange
        var editor = await _factory.CreateClientWithRoleAsync(Roles.Editor);
        var nieuw = NieuwProject();

        // Act
        var response = await editor.PostAsJsonAsync(Url, nieuw);

        // Assert: 201 met de juiste gegevens en een Id van de database
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var aangemaakt = await response.Content.ReadFromJsonAsync<ProjectResponse>();
        Assert.NotNull(aangemaakt);
        Assert.True(aangemaakt.Id > 0);
        Assert.Equal(nieuw.Titel, aangemaakt.Titel);
        Assert.Equal(nieuw.Beschrijving, aangemaakt.Beschrijving);

        // Assert: het project is echt opgeslagen
        var opgehaald = await _factory.CreateClient()
            .GetFromJsonAsync<ProjectResponse>($"{Url}/{aangemaakt.Id}");
        Assert.Equal(nieuw.Titel, opgehaald!.Titel);
    }

    [Fact]
    public async Task Create_AlsAdmin_Geeft201()
    {
        // Arrange
        var admin = await _factory.CreateClientWithRoleAsync(Roles.Admin);
        var nieuw = NieuwProject();

        // Act
        var response = await admin.PostAsJsonAsync(Url, nieuw);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var aangemaakt = await response.Content.ReadFromJsonAsync<ProjectResponse>();
        Assert.Equal(nieuw.Titel, aangemaakt!.Titel);
    }

    [Fact]
    public async Task Create_ZonderAanmelding_Geeft401EnSlaatNietsOp()
    {
        // Arrange
        var client = _factory.CreateClient();
        var nieuw = NieuwProject();

        // Act
        var response = await client.PostAsJsonAsync(Url, nieuw);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var alle = await client.GetFromJsonAsync<List<ProjectResponse>>(Url);
        Assert.DoesNotContain(alle!, p => p.Titel == nieuw.Titel);
    }

    [Fact]
    public async Task Create_AlsGast_Geeft403EnSlaatNietsOp()
    {
        // Arrange
        var gast = await _factory.CreateClientWithRoleAsync(Roles.Gast);
        var nieuw = NieuwProject();

        // Act
        var response = await gast.PostAsJsonAsync(Url, nieuw);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var alle = await gast.GetFromJsonAsync<List<ProjectResponse>>(Url);
        Assert.DoesNotContain(alle!, p => p.Titel == nieuw.Titel);
    }

    [Fact]
    public async Task Create_OngeldigeInvoer_Geeft400()
    {
        // Arrange: lege titel, terwijl Titel [Required] is
        var editor = await _factory.CreateClientWithRoleAsync(Roles.Editor);
        var ongeldig = NieuwProject();
        ongeldig.Titel = "";

        // Act
        var response = await editor.PostAsJsonAsync(Url, ongeldig);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ===== PUT =====

    [Fact]
    public async Task Update_AlsEditor_Geeft200EnWijzigtProject()
    {
        // Arrange
        var bestaand = await MaakProjectAanAlsEditorAsync();
        var editor = await _factory.CreateClientWithRoleAsync(Roles.Editor);
        var wijziging = new UpdateProjectRequest
        {
            Titel = "Gewijzigde titel",
            Beschrijving = "Gewijzigde beschrijving",
            Categorie = "C#",
            GitHubUrl = null,
            Datum = new DateTime(2026, 11, 15)
        };

        // Act
        var response = await editor.PutAsJsonAsync($"{Url}/{bestaand.Id}", wijziging);

        // Assert: 200 met de nieuwe gegevens
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var gewijzigd = await response.Content.ReadFromJsonAsync<ProjectResponse>();
        Assert.Equal("Gewijzigde titel", gewijzigd!.Titel);

        // Assert: de wijziging is echt opgeslagen
        var opgehaald = await _factory.CreateClient()
            .GetFromJsonAsync<ProjectResponse>($"{Url}/{bestaand.Id}");
        Assert.Equal("Gewijzigde titel", opgehaald!.Titel);
        Assert.Equal("C#", opgehaald.Categorie);
    }

    [Fact]
    public async Task Update_AlsGast_Geeft403EnWijzigtNiets()
    {
        // Arrange
        var bestaand = await MaakProjectAanAlsEditorAsync();
        var gast = await _factory.CreateClientWithRoleAsync(Roles.Gast);
        var wijziging = new UpdateProjectRequest
        {
            Titel = "Mag niet",
            Beschrijving = "Mag niet",
            Categorie = "Test",
            Datum = new DateTime(2026, 11, 15)
        };

        // Act
        var response = await gast.PutAsJsonAsync($"{Url}/{bestaand.Id}", wijziging);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var opgehaald = await gast.GetFromJsonAsync<ProjectResponse>($"{Url}/{bestaand.Id}");
        Assert.Equal(bestaand.Titel, opgehaald!.Titel);
    }

    [Fact]
    public async Task Update_OnbekendId_Geeft404()
    {
        // Arrange
        var editor = await _factory.CreateClientWithRoleAsync(Roles.Editor);
        var wijziging = new UpdateProjectRequest
        {
            Titel = "Bestaat niet",
            Beschrijving = "Bestaat niet",
            Categorie = "Test",
            Datum = new DateTime(2026, 11, 15)
        };

        // Act
        var response = await editor.PutAsJsonAsync($"{Url}/{OnbekendId}", wijziging);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ===== DELETE =====

    [Fact]
    public async Task Delete_AlsAdmin_Geeft204EnVerwijdertProject()
    {
        // Arrange
        var bestaand = await MaakProjectAanAlsEditorAsync();
        var admin = await _factory.CreateClientWithRoleAsync(Roles.Admin);

        // Act
        var response = await admin.DeleteAsync($"{Url}/{bestaand.Id}");

        // Assert: 204 en het project is weg
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var opgehaald = await admin.GetAsync($"{Url}/{bestaand.Id}");
        Assert.Equal(HttpStatusCode.NotFound, opgehaald.StatusCode);
    }

    [Fact]
    public async Task Delete_ZonderAanmelding_Geeft401EnProjectBestaatNog()
    {
        // Arrange
        var bestaand = await MaakProjectAanAlsEditorAsync();
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