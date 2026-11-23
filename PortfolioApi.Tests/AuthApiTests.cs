using System.Net;
using System.Net.Http.Json;
using PortfolioApi.Auth;
using PortfolioApi.Dtos;

namespace PortfolioApi.Tests;

[Collection("Api")]
public class AuthApiTests
{
    private const string RegisterUrl = "/api/auth/register";
    private const string LoginUrl = "/api/auth/login";
    private const string MeUrl = "/api/auth/me";

    private readonly PortfolioApiFactory _factory;

    public AuthApiTests(PortfolioApiFactory factory)
    {
        _factory = factory;
    }

    // Een uniek e-mailadres per test
    private static string UniekEmail()
    {
        return $"nieuw-{Guid.NewGuid():N}@test.local";
    }

    // ===== Registratie =====

    [Fact]
    public async Task Register_NieuweGebruiker_Geeft200EnAlleenRolGast()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new RegisterUserRequest
        {
            Email = UniekEmail(),
            Password = PortfolioApiFactory.CreateTestPassword()
        };

        // Act
        var response = await client.PostAsJsonAsync(RegisterUrl, request);

        // Assert: 200 en de nieuwe gebruiker heeft alleen de rol Gast
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var gebruiker = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(gebruiker);
        Assert.Equal(request.Email, gebruiker.Email);
        Assert.Equal(new[] { Roles.Gast }, gebruiker.Roles);
    }

    [Fact]
    public async Task Register_ZwakWachtwoord_Geeft400()
    {
        // Arrange: "abc" voldoet niet aan de wachtwoordregels van Identity
        var client = _factory.CreateClient();
        var request = new RegisterUserRequest
        {
            Email = UniekEmail(),
            Password = "abc"
        };

        // Act
        var response = await client.PostAsJsonAsync(RegisterUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_EmailAlInGebruik_Geeft400()
    {
        // Arrange: eerst een keer registreren
        var client = _factory.CreateClient();
        var request = new RegisterUserRequest
        {
            Email = UniekEmail(),
            Password = PortfolioApiFactory.CreateTestPassword()
        };
        var eerste = await client.PostAsJsonAsync(RegisterUrl, request);
        eerste.EnsureSuccessStatusCode();

        // Act: nog een keer registreren met hetzelfde e-mailadres
        var response = await client.PostAsJsonAsync(RegisterUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ===== Login =====

    [Fact]
    public async Task Login_JuisteGegevens_Geeft200EnWerkendeCookie()
    {
        // Arrange: een geregistreerde gebruiker
        var client = _factory.CreateClient();
        var email = UniekEmail();
        var password = PortfolioApiFactory.CreateTestPassword();
        var registratie = await client.PostAsJsonAsync(RegisterUrl, new RegisterUserRequest
        {
            Email = email,
            Password = password
        });
        registratie.EnsureSuccessStatusCode();

        // Act
        var response = await client.PostAsJsonAsync(LoginUrl, new LoginUserRequest
        {
            Email = email,
            Password = password
        });

        // Assert: 200 en de server stuurt een authenticatiecookie
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookies));
        Assert.Contains(cookies, c => c.StartsWith(".AspNetCore.Identity.Application"));

        // Assert: met die cookie weet de API wie we zijn, en welke rol we hebben
        var ik = await client.GetFromJsonAsync<UserResponse>(MeUrl);
        Assert.Equal(email, ik!.Email);
        Assert.Contains(Roles.Gast, ik.Roles);
        Assert.DoesNotContain(Roles.Editor, ik.Roles);
        Assert.DoesNotContain(Roles.Admin, ik.Roles);
    }

    [Fact]
    public async Task Login_FoutWachtwoord_Geeft401()
    {
        // Arrange: een geregistreerde gebruiker
        var client = _factory.CreateClient();
        var email = UniekEmail();
        var registratie = await client.PostAsJsonAsync(RegisterUrl, new RegisterUserRequest
        {
            Email = email,
            Password = PortfolioApiFactory.CreateTestPassword()
        });
        registratie.EnsureSuccessStatusCode();

        // Act: inloggen met een ander wachtwoord
        var response = await client.PostAsJsonAsync(LoginUrl, new LoginUserRequest
        {
            Email = email,
            Password = PortfolioApiFactory.CreateTestPassword()
        });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_ZonderAanmelding_Geeft401()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(MeUrl);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}