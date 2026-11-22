using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortfolioApi.Entities;

namespace PortfolioApi.Data;

public class PortfolioDbContext : IdentityDbContext<User>
{
    public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<Blogpost> Blogposts => Set<Blogpost>();
}