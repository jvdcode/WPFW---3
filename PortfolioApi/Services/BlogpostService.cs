using Microsoft.EntityFrameworkCore;
using PortfolioApi.Data;
using PortfolioApi.Dtos;
using PortfolioApi.Entities;

namespace PortfolioApi.Services;

public class BlogpostService : IBlogpostService
{
    private readonly PortfolioDbContext _db;

    // De DbContext komt binnen via dependency injection (constructorinjectie)
    public BlogpostService(PortfolioDbContext db)
    {
        _db = db;
    }

    // GET alle
    public async Task<List<BlogpostResponse>> GetAllAsync()
    {
        var blogposts = await _db.Blogposts
            .AsNoTracking()
            .ToListAsync();

        return blogposts.Select(ToResponse).ToList();
    }

    // GET op id
    public async Task<BlogpostResponse?> GetByIdAsync(int id)
    {
        var blogpost = await _db.Blogposts
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);

        if (blogpost is null)
        {
            return null;
        }

        return ToResponse(blogpost);
    }

    // POST
    public async Task<BlogpostResponse> CreateAsync(CreateBlogpostRequest request)
    {
        // Omzetting DTO → entiteit
        var blogpost = new Blogpost
        {
            Titel = request.Titel,
            Inhoud = request.Inhoud,
            Publicatiedatum = request.Publicatiedatum
        };

        _db.Blogposts.Add(blogpost);
        await _db.SaveChangesAsync();

        // Na SaveChangesAsync heeft blogpost een Id van de database
        return ToResponse(blogpost);
    }

    // PUT
    public async Task<BlogpostResponse?> UpdateAsync(int id, UpdateBlogpostRequest request)
    {
        var blogpost = await _db.Blogposts.FirstOrDefaultAsync(b => b.Id == id);

        if (blogpost is null)
        {
            return null;
        }

        blogpost.Titel = request.Titel;
        blogpost.Inhoud = request.Inhoud;
        blogpost.Publicatiedatum = request.Publicatiedatum;

        await _db.SaveChangesAsync();

        return ToResponse(blogpost);
    }

    // DELETE
    public async Task<bool> DeleteAsync(int id)
    {
        var blogpost = await _db.Blogposts.FirstOrDefaultAsync(b => b.Id == id);

        if (blogpost is null)
        {
            return false;
        }

        _db.Blogposts.Remove(blogpost);
        await _db.SaveChangesAsync();

        return true;
    }

    // Omzetting entiteit → DTO
    private static BlogpostResponse ToResponse(Blogpost blogpost)
    {
        return new BlogpostResponse(
            blogpost.Id,
            blogpost.Titel,
            blogpost.Inhoud,
            blogpost.Publicatiedatum);
    }
}