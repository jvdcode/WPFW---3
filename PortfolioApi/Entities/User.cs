using Microsoft.AspNetCore.Identity;

namespace PortfolioApi.Entities;

public class User : IdentityUser
{
    // Extra velden kun je hier later toevoegen.
    // Id, UserName, Email en PasswordHash zitten al in IdentityUser.
}