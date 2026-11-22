using Microsoft.AspNetCore.Authorization;                  
using Microsoft.AspNetCore.Mvc;
using PortfolioApi.Auth;                                   
using PortfolioApi.Dtos;
using PortfolioApi.Services;

namespace PortfolioApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    // GET: api/projects  → publiek
    [HttpGet]
    public async Task<ActionResult<List<ProjectResponse>>> GetAll()
    {
        var projects = await _projectService.GetAllAsync();
        return Ok(projects);
    }

    // GET: api/projects/5  → publiek
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectResponse>> GetById(int id)
    {
        var project = await _projectService.GetByIdAsync(id);

        if (project is null)
        {
            return NotFound();
        }

        return Ok(project);
    }

    // POST: api/projects  → alleen Editor of Admin
    [HttpPost]
    [Authorize(Policy = Policies.CanEditContent)]           
    public async Task<ActionResult<ProjectResponse>> Create(CreateProjectRequest request)
    {
        var project = await _projectService.CreateAsync(request);

        return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
    }

    // PUT: api/projects/5  → alleen Editor of Admin
    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.CanEditContent)]           
    public async Task<ActionResult<ProjectResponse>> Update(int id, UpdateProjectRequest request)
    {
        var project = await _projectService.UpdateAsync(id, request);

        if (project is null)
        {
            return NotFound();
        }

        return Ok(project);
    }

    // DELETE: api/projects/5  → alleen Editor of Admin
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.CanEditContent)]           
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _projectService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}