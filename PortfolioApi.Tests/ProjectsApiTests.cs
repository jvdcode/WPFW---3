using System.Net;

namespace PortfolioApi.Tests;

[Collection("Api")]
public class ProjectsApiTests
{
    private readonly PortfolioApiFactory _factory;

    // xUnit geeft de gedeelde fabriek mee via de constructor
    public ProjectsApiTests(PortfolioApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_ZonderAanmelding_Geeft200()
    {
        // Arrange: een client die niet is ingelogd
        var client = _factory.CreateClient();

        // Act: GET /api/projects
        var response = await client.GetAsync("/api/projects");

        // Assert: publieke route, dus 200 OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}