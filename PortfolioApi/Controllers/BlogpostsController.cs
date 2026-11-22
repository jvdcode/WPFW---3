using Microsoft.AspNetCore.Authorization;                  
using Microsoft.AspNetCore.Mvc;
using PortfolioApi.Auth;                                   
using PortfolioApi.Dtos;
using PortfolioApi.Services;

namespace PortfolioApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BlogpostsController : ControllerBase
{
    private readonly IBlogpostService _blogpostService;

    public BlogpostsController(IBlogpostService blogpostService)
    {
        _blogpostService = blogpostService;
    }

    // GET: api/blogposts  → publiek
    [HttpGet]
    public async Task<ActionResult<List<BlogpostResponse>>> GetAll()
    {
        var blogposts = await _blogpostService.GetAllAsync();
        return Ok(blogposts);
    }

    // GET: api/blogposts/5  → publiek
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BlogpostResponse>> GetById(int id)
    {
        var blogpost = await _blogpostService.GetByIdAsync(id);

        if (blogpost is null)
        {
            return NotFound();
        }

        return Ok(blogpost);
    }

    // POST: api/blogposts  → alleen Editor of Admin
    [HttpPost]
    [Authorize(Policy = Policies.CanEditContent)]           
    public async Task<ActionResult<BlogpostResponse>> Create(CreateBlogpostRequest request)
    {
        var blogpost = await _blogpostService.CreateAsync(request);

        return CreatedAtAction(nameof(GetById), new { id = blogpost.Id }, blogpost);
    }

    // PUT: api/blogposts/5  → alleen Editor of Admin
    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.CanEditContent)]           
    public async Task<ActionResult<BlogpostResponse>> Update(int id, UpdateBlogpostRequest request)
    {
        var blogpost = await _blogpostService.UpdateAsync(id, request);

        if (blogpost is null)
        {
            return NotFound();
        }

        return Ok(blogpost);
    }

    // DELETE: api/blogposts/5  → alleen Editor of Admin
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.CanEditContent)]           
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _blogpostService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}