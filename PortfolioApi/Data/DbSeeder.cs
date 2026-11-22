using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortfolioApi.Auth;
using PortfolioApi.Entities;

namespace PortfolioApi.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        // Een scope maken, zodat we scoped services (DbContext, UserManager) kunnen ophalen
        await using var scope = services.CreateAsyncScope();

        // 1. Openstaande migraties toepassen
        var db = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
        await db.Database.MigrateAsync();

        // 2. Rollen aanmaken als ze nog niet bestaan
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 3. Lokale Editor- en Admin-gebruiker aanmaken (gegevens uit user-secrets)
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        await CreateUserWithRoleAsync(
            userManager,
            configuration["SeedUsers:Editor:Email"],
            configuration["SeedUsers:Editor:Password"],
            Roles.Editor);

        await CreateUserWithRoleAsync(
            userManager,
            configuration["SeedUsers:Admin:Email"],
            configuration["SeedUsers:Admin:Password"],
            Roles.Admin);
    }

    private static async Task CreateUserWithRoleAsync(
        UserManager<User> userManager, string? email, string? password, string role)
    {
        // Niet ingesteld in user-secrets? Dan slaan we deze gebruiker over.
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new User
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Seed-gebruiker {email} kon niet worden aangemaakt: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}