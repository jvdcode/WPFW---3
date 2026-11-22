namespace PortfolioApi.Dtos;

public record UserResponse(string Email, IEnumerable<string> Roles);