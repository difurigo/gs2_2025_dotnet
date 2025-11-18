using CareerHub.Api.Data;
using CareerHub.Api.Dtos;
using CareerHub.Api.Models;
using CareerHub.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerHub.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<UserAccount> _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthController(AppDbContext context, IPasswordHasher<UserAccount> passwordHasher, ITokenService tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant());
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var token = _tokenService.Generate(user);
        return Ok(new AuthResponse(token.Token, token.ExpiresAt));
    }
}
