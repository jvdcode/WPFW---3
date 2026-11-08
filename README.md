# PortfolioApi

Backend voor mijn portfoliosite (WPFW – Opdracht 3).
Een ASP.NET Core Web API met volledige CRUD voor **projecten** en **blogposts**.
De gegevens worden via Entity Framework Core opgeslagen in SQL Server.

## Technische gegevens

| Onderdeel | Keuze |
|---|---|
| .NET-versie | **.NET 10** |
| API-stijl | **Controllers** (`[ApiController]` + `ControllerBase`) |
| Architectuur | Meerdere lagen: Controllers → Services → Data |
| Database | SQL Server Express 2022 |
| ORM | Entity Framework Core 10 (`Microsoft.EntityFrameworkCore.SqlServer`) |
| API-documentatie | Swagger UI (`Swashbuckle.AspNetCore`) |
| Database aanmaken | **Migraties** (`dotnet ef`) |

## Codestructuur

```
PortfolioApi/
├── Controllers/   HTTP-afhandeling: routes en statuscodes
├── Services/      Applicatielogica + omzetting tussen entiteit en DTO
├── Data/          PortfolioDbContext (EF Core-toegang tot SQL Server)
├── Entities/      Project en Blogpost: wat in de database wordt opgeslagen
├── Dtos/          Invoer- en uitvoer-DTO's: wat de client verstuurt en ontvangt
├── Migrations/    Door EF Core gegenereerde migraties
└── Program.cs     Registratie van services (DI) en de middleware-pipeline
```

Een request loopt altijd door dezelfde lagen:

```
Swagger UI → Controller → Service → PortfolioDbContext → SQL Server
```

De controllers geven nooit een EF Core-entiteit terug. De service zet entiteiten om naar DTO's
(`ToResponse`) en DTO's naar entiteiten (in `CreateAsync` en `UpdateAsync`).

## Endpoints

| Methode | Route | Resultaat |
|---|---|---|
| GET | `/api/projects` | 200 + lijst met projecten |
| GET | `/api/projects/{id}` | 200 + project, of 404 |
| POST | `/api/projects` | 201 + nieuw project (400 bij ongeldige invoer) |
| PUT | `/api/projects/{id}` | 200 + gewijzigd project, of 404 |
| DELETE | `/api/projects/{id}` | 204, of 404 |
| GET | `/api/blogposts` | 200 + lijst met blogposts |
| GET | `/api/blogposts/{id}` | 200 + blogpost, of 404 |
| POST | `/api/blogposts` | 201 + nieuwe blogpost (400 bij ongeldige invoer) |
| PUT | `/api/blogposts/{id}` | 200 + gewijzigde blogpost, of 404 |
| DELETE | `/api/blogposts/{id}` | 204, of 404 |

## Vereisten

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (bijvoorbeeld SQL Server Express) met Windows Authentication
- De EF Core-tool, versie 10:
  ```powershell
  dotnet tool install --global dotnet-ef --version 10.0.14
  ```

## SQL Server configureren

De connectionstring staat in `PortfolioApi/appsettings.json`:

```json
"ConnectionStrings": {
  "PortfolioDb": "Server=localhost\\SQLEXPRESS;Database=PortfolioDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

- `Server=localhost\\SQLEXPRESS`: de naam van de SQL Server-instantie. Heet jouw instantie anders,
  pas dit dan aan (bijvoorbeeld `Server=localhost` of `Server=(localdb)\\MSSQLLocalDB`).
- `Trusted_Connection=True`: inloggen met je Windows-account. Er staat daardoor geen wachtwoord in de code.
- `Database=PortfolioDb`: deze database wordt door de migratie aangemaakt.

In `Program.cs` wordt de DbContext geregistreerd met de SQL Server-provider:

```csharp
builder.Services.AddDbContext<PortfolioDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PortfolioDb")));
```

## Database aanmaken

De database wordt aangemaakt met een **EF Core-migratie** (niet met `EnsureCreated`).
De migratie `InitialCreate` staat al in de map `Migrations`. Voer in de projectmap uit:

```powershell
cd PortfolioApi
dotnet ef database update
```

Dit maakt de database `PortfolioDb` aan met de tabellen `Projects` en `Blogposts`.

## Applicatie starten

```powershell
cd PortfolioApi
dotnet run
```

Open daarna Swagger UI in de browser:

```
http://localhost:5225/swagger
```

De gegevens blijven bewaard na een herstart van de applicatie, omdat ze in SQL Server staan.