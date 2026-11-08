using PortfolioApi.Dtos;

namespace PortfolioApi.Services;

public interface IBlogpostService
{
    Task<List<BlogpostResponse>> GetAllAsync();

    Task<BlogpostResponse?> GetByIdAsync(int id);

    Task<BlogpostResponse> CreateAsync(CreateBlogpostRequest request);

    Task<BlogpostResponse?> UpdateAsync(int id, UpdateBlogpostRequest request);

    Task<bool> DeleteAsync(int id);
}