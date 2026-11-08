using Microsoft.EntityFrameworkCore;
using PortfolioApi.Data;
using PortfolioApi.Dtos;
using PortfolioApi.Entities;

namespace PortfolioApi.Services;

public class ProjectService : IProjectService
{
    private readonly PortfolioDbContext _db;

    // De DbContext komt binnen via dependency injection (constructorinjectie)
    public ProjectService(PortfolioDbContext db)
    {
        _db = db;
    }

    // GET alle
    public async Task<List<ProjectResponse>> GetAllAsync()
    {
        var projects = await _db.Projects
            .AsNoTracking()
            .ToListAsync();

        return projects.Select(ToResponse).ToList();
    }

    // GET op id
    public async Task<ProjectResponse?> GetByIdAsync(int id)
    {
        var project = await _db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project is null)
        {
            return null;
        }

        return ToResponse(project);
    }

    // POST
    public async Task<ProjectResponse> CreateAsync(CreateProjectRequest request)
    {
        // Omzetting DTO → entiteit
        var project = new Project
        {
            Titel = request.Titel,
            Beschrijving = request.Beschrijving,
            Categorie = request.Categorie,
            GitHubUrl = request.GitHubUrl,
            Datum = request.Datum
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        // Na SaveChangesAsync heeft project een Id van de database
        return ToResponse(project);
    }

    // PUT
    public async Task<ProjectResponse?> UpdateAsync(int id, UpdateProjectRequest request)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id);

        if (project is null)
        {
            return null;
        }

        project.Titel = request.Titel;
        project.Beschrijving = request.Beschrijving;
        project.Categorie = request.Categorie;
        project.GitHubUrl = request.GitHubUrl;
        project.Datum = request.Datum;

        await _db.SaveChangesAsync();

        return ToResponse(project);
    }

    // DELETE
    public async Task<bool> DeleteAsync(int id)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id);

        if (project is null)
        {
            return false;
        }

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();

        return true;
    }

    // Omzetting entiteit → DTO
    private static ProjectResponse ToResponse(Project project)
    {
        return new ProjectResponse(
            project.Id,
            project.Titel,
            project.Beschrijving,
            project.Categorie,
            project.GitHubUrl,
            project.Datum);
    }
}