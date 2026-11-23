using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PortfolioApi.Data;
using PortfolioApi.Dtos;
using PortfolioApi.Entities;

namespace PortfolioApi.Tests;

// Start de echte API in het geheugen, maar met een aparte testdatabase
public class PortfolioApiFactory : WebApplicationFactory<Program>
{
    private const string TestConnectionString =
        "Server=localhost\\SQLEXPRESS;Database=PortfolioDb_Test;Trusted_Connection=True;TrustServerCertificate=True";

    public PortfolioApiFactory()
    {
        // Elke testrun begint met een lege testdatabase.
        // De seeder in Program.cs maakt hem daarna opnieuw aan (migraties + rollen).
        var options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseSqlServer(TestConnectionString)
            .Options;

        using var db = new PortfolioDbContext(options);
        db.Database.EnsureDeleted();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Omgeving "Testing": geen Swagger en geen user-secrets
        builder.UseEnvironment("Testing");

        // De connectionstring vervangen door die van de testdatabase
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PortfolioDb"] = TestConnectionString
            });
        });
    }

    // ===== Hulpmethodes voor de tests =====

    // Maakt een willekeurig wachtwoord dat aan de Identity-regels voldoet.
    // Zo staat er geen vast wachtwoord in de repository.
    public static string CreateTestPassword()
    {
        return $"Test-{Guid.NewGuid():N}aA1!";
    }

    // Maakt een unieke gebruiker met de gegeven rol en geeft een ingelogde client terug
    public async Task<HttpClient> CreateClientWithRoleAsync(string role)
    {
        var email = $"{role.ToLower()}-{Guid.NewGuid():N}@test.local";
        var password = CreateTestPassword();

        // Gebruiker direct aanmaken via de UserManager (net als de seeder)
        using (var scope = Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            var user = new User
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Testgebruiker kon niet worden aangemaakt.");
            }

            await userManager.AddToRoleAsync(user, role);
        }

        // Inloggen via de echte login-route, zodat de client de cookie krijgt
        return await CreateLoggedInClientAsync(email, password);
    }

    // Logt in via POST /api/auth/login en geeft de client met cookie terug
    public async Task<HttpClient> CreateLoggedInClientAsync(string email, string password)
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginUserRequest
        {
            Email = email,
            Password = password
        });

        response.EnsureSuccessStatusCode();

        return client;
    }
}