using EmployeesApi.Data;
using EmployeesApi.Models;
using EmployeesApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeesApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly AuthService _authService;

    public AuthController(AppDbContext context, AuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] User user)
    {
        if (_context.Users.Any(x => x.Username == user.Username))
            return BadRequest(new { ErrorMessage = "A user with this login already exists" });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok("User registered successfully");
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest login)
    {
        await CheckAndSetDefaultUser();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == login.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
        {
            return Unauthorized();
        }

        var token = _authService.GenerateToken(user);
        return Ok(new { Token = token });
    }

    private async Task CheckAndSetDefaultUser()
    {
        if (!await _context.Users.AnyAsync())
        {
            var defaultUser = new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!")
            };
            
            _context.Users.Add(defaultUser);
            await _context.SaveChangesAsync();
        }
    }

    public record LoginRequest(string Username, string Password);
}
