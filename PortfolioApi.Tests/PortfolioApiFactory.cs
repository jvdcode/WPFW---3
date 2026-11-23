using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PortfolioApi.Data;

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
}