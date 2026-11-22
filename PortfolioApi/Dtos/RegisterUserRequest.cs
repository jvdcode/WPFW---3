using System.ComponentModel.DataAnnotations;

namespace PortfolioApi.Dtos;

public class RegisterUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}