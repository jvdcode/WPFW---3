# PortfolioApi

Backend voor mijn portfoliosite (WPFW – Opdracht 3 en 4).
Een ASP.NET Core Web API met volledige CRUD voor **projecten** en **blogposts**,
beveiligd met **ASP.NET Core Identity** (rollen Gast, Editor en Admin) en getest met **xUnit**.

## Technische gegevens

| Onderdeel | Keuze |
|---|---|
| .NET-versie | **.NET 10** |
| API-stijl | **Controllers** (`[ApiController]` + `ControllerBase`) |
| Architectuur | Lagen: Controllers → Services → Data |
| Database | SQL Server Express 2022 |
| ORM | Entity Framework Core 10 (`Microsoft.EntityFrameworkCore.SqlServer`) |
| Authenticatie | ASP.NET Core Identity met EF Core, inloggen met een **authenticatiecookie** |
| Autorisatie | Policy `CanEditContent` = rol **Editor** of **Admin** |
| Tests | xUnit + `WebApplicationFactory` (HTTP-tests met een aparte testdatabase) |
| API-documentatie | Swagger UI (`Swashbuckle.AspNetCore`) |

## Codestructuur

```
PortfolioApi/                 De Web API
├── Auth/                     Rolnamen (Roles) en policynamen (Policies)
├── Controllers/              HTTP-afhandeling: Projects, Blogposts en Auth
├── Services/                 Applicatielogica + omzetting tussen entiteit en DTO
├── Data/                     PortfolioDbContext (IdentityDbContext) en DbSeeder
├── Entities/                 Project, Blogpost en User (IdentityUser)
├── Dtos/                     Invoer- en uitvoer-DTO's
├── Migrations/               InitialCreate en AddIdentity
└── Program.cs                DI-registratie, Identity, policy en middleware-pipeline

PortfolioApi.Tests/           Het xUnit-testproject
├── PortfolioApiFactory.cs    Start de API met de testdatabase + hulpmethodes om in te loggen
├── ApiCollection.cs          Alle tests delen één fabriek en draaien na elkaar
├── ProjectsApiTests.cs       14 tests voor de project-routes
├── BlogpostsApiTests.cs      14 tests voor de blogpost-routes
└── AuthApiTests.cs           6 tests voor registratie, login en /me
```



## Endpoints en toegang

| Methode | Route | Toegang | Resultaat |
|---|---|---|---|
| GET | `/api/projects` | Iedereen | 200 |
| GET | `/api/projects/{id}` | Iedereen | 200 of 404 |
| POST | `/api/projects` | Editor, Admin | 201 · 400 · 401 · 403 |
| PUT | `/api/projects/{id}` | Editor, Admin | 200 · 400 · 401 · 403 · 404 |
| DELETE | `/api/projects/{id}` | Editor, Admin | 204 · 401 · 403 · 404 |
| GET | `/api/blogposts` | Iedereen | 200 |
| GET | `/api/blogposts/{id}` | Iedereen | 200 of 404 |
| POST | `/api/blogposts` | Editor, Admin | 201 · 400 · 401 · 403 |
| PUT | `/api/blogposts/{id}` | Editor, Admin | 200 · 400 · 401 · 403 · 404 |
| DELETE | `/api/blogposts/{id}` | Editor, Admin | 204 · 401 · 403 · 404 |
| POST | `/api/auth/register` | Iedereen | 200 (rol Gast) of 400 |
| POST | `/api/auth/login` | Iedereen | 200 + cookie, of 401 |
| POST | `/api/auth/logout` | Ingelogd | 204 of 401 |
| GET | `/api/auth/me` | Ingelogd | 200 of 401 |

- **401 Unauthorized**: niet ingelogd.
- **403 Forbidden**: wel ingelogd, maar zonder de rol Editor of Admin.

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

## Database en migraties

De database wordt aangemaakt met **EF Core-migraties** (niet met `EnsureCreated`):

| Migratie | Wat |
|---|---|
| `InitialCreate` | Tabellen `Projects` en `Blogposts` |
| `AddIdentity` | De Identity-tabellen: `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, enzovoort |

Migraties toepassen:

```powershell
cd PortfolioApi
dotnet ef database update
```

De `DbSeeder` past openstaande migraties ook automatisch toe bij het opstarten van de API.

## Gebruikers en rollen lokaal beschikbaar maken

Er zijn drie rollen: **Gast**, **Editor** en **Admin**. De `DbSeeder` maakt ze aan bij het opstarten.

### Gewone gebruiker (Gast)

Registreer via `POST /api/auth/register`. Een nieuwe gebruiker krijgt **altijd** de rol **Gast**.
Via publieke registratie kun je nooit Editor of Admin worden.

### Editor en Admin (lokaal script)

De seeder maakt bij het opstarten een Editor- en een Admin-account aan. E-mail en wachtwoord komen uit
**user-secrets**. Die staan alleen op je eigen computer en **niet in de repository**.

```powershell
cd PortfolioApi
dotnet user-secrets init
dotnet user-secrets set "SeedUsers:Editor:Email" "editor@portfolio.local"
dotnet user-secrets set "SeedUsers:Editor:Password" "<kies een wachtwoord>"
dotnet user-secrets set "SeedUsers:Admin:Email" "admin@portfolio.local"
dotnet user-secrets set "SeedUsers:Admin:Password" "<kies een wachtwoord>"
```

Een wachtwoord moet minimaal 6 tekens hebben, met een hoofdletter, een kleine letter, een cijfer
en een speciaal teken. Start daarna de API (`dotnet run`). De seeder maakt de accounts aan en kent de rollen toe.

### Alternatief: een bestaande gebruiker handmatig Editor maken

In SSMS, op de database `PortfolioDb`:

```sql
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.Id, r.Id
FROM AspNetUsers u, AspNetRoles r
WHERE u.Email = 'bezoeker@portfolio.local' AND r.Name = 'Editor';
```

### Let op: opnieuw inloggen na een rolwijziging

De rollen worden bij het inloggen in de authenticatiecookie gezet. Na een wijziging van een rol
moet de gebruiker **uitloggen en opnieuw inloggen** om de nieuwe rechten te krijgen.

## Applicatie starten

```powershell
cd PortfolioApi
dotnet run
```

Open daarna Swagger UI in de browser:

```
http://localhost:5225/swagger
```

Inloggen in Swagger: gebruik `POST /api/auth/login`. De browser bewaart de cookie en stuurt
hem automatisch mee bij volgende requests.

## Tests uitvoeren

Vanuit de hoofdmap van de repository:

```powershell
dotnet test
```

Met de naam van elke test in de uitvoer:

```powershell
dotnet test --logger "console;verbosity=normal"
```

- Het testproject staat in [`PortfolioApi.Tests`](PortfolioApi.Tests).
- De tests starten de echte API met `WebApplicationFactory` en sturen echte HTTP-requests.
- De tests gebruiken een **aparte database**, `PortfolioDb_Test`. Die wordt bij elke testrun verwijderd en
  opnieuw aangemaakt. De gewone database `PortfolioDb` wordt niet aangeraakt.
- Testgebruikers krijgen een willekeurig wachtwoord. Er staan geen vaste wachtwoorden in de code.
- Vereiste: SQL Server draait lokaal op `localhost\SQLEXPRESS`.