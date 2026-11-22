using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PortfolioApi.Auth;
using PortfolioApi.Dtos;
using PortfolioApi.Entities;

namespace PortfolioApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    // UserManager en SignInManager komen van Identity, via dependency injection
    public AuthController(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register(RegisterUserRequest request)
    {
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email
        };

        // UserManager maakt de gebruiker aan en slaat een hash van het wachtwoord op
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            // Bijvoorbeeld: wachtwoord te zwak of e-mail al in gebruik → 400
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return ValidationProblem(ModelState);
        }

        // Iedere nieuwe gebruiker krijgt altijd de rol Gast
        await _userManager.AddToRoleAsync(user, Roles.Gast);

        return Ok(new UserResponse(user.Email!, new[] { Roles.Gast }));
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<UserResponse>> Login(LoginUserRequest request)
    {
        // Controleert het wachtwoord en zet bij succes een authenticatiecookie
        var result = await _signInManager.PasswordSignInAsync(
            request.Email,
            request.Password,
            isPersistent: false,
            lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        var roles = await _userManager.GetRolesAsync(user!);

        return Ok(new UserResponse(user!.Email!, roles));
    }

    // POST: api/auth/logout
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return NoContent();
    }

    // GET: api/auth/me
    [Authorize]
    [HttpGet("me")]
    public ActionResult<UserResponse> Me()
    {
        // User = de ClaimsPrincipal die ASP.NET Core uit de cookie heeft gemaakt (dia 22)
        var email = User.Identity?.Name ?? string.Empty;
        var roles = User.FindAll(ClaimTypes.Role).Select(claim => claim.Value);

        return Ok(new UserResponse(email, roles));
    }
}