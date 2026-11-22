using Microsoft.AspNetCore.Identity;                       
using Microsoft.EntityFrameworkCore;
using PortfolioApi.Data;
using PortfolioApi.Entities;                               
using PortfolioApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ===== 1. Services toevoegen (vóór Build) =====
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PortfolioDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PortfolioDb")));

// NIEUW: Identity met rollen, opgeslagen via EF Core in onze database
builder.Services.AddIdentityApiEndpoints<User>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<PortfolioDbContext>();

builder.Services.AddAuthentication();                      
builder.Services.AddAuthorization();                       

builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IBlogpostService, BlogpostService>();

var app = builder.Build();

// ===== 2. Middleware pipeline (ná Build) =====
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();                                   
app.UseAuthorization();                                    
app.MapControllers();

app.Run();