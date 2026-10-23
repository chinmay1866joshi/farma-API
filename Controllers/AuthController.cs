using System.Security.Claims;
using FarmaApi.Data;
using FarmaApi.DTOs.Auth;
using FarmaApi.Models;
using FarmaApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AppDbContext context, ITokenService tokenService, ILogger<AuthController> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user (Farmer or Customer)
    /// </summary>
    [HttpPost("signup")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Signup([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == normalizedEmail);

        if (existingUser)
        {
            return Conflict(new { message = "An account with this email already exists." });
        }

        var normalizedRole = request.Role.Trim().ToLowerInvariant();
        if (normalizedRole != "farmer" && normalizedRole != "customer" && normalizedRole != "admin")
        {
            normalizedRole = "customer";
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            MobileNumber = request.MobileNumber.Trim(),
            Age = request.Age,
            Role = normalizedRole,
            City = request.City.Trim(),
            State = request.State.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _tokenService.CreateToken(user, rememberMe: false);
        var expiry = _tokenService.GetTokenExpiryMinutes(rememberMe: false);

        var response = new AuthResponseDto
        {
            Token = token,
            TokenType = "Bearer",
            ExpiresInMinutes = expiry,
            User = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                Age = user.Age,
                Role = user.Role,
                City = user.City,
                State = user.State,
                CreatedAt = user.CreatedAt
            }
        };

        _logger.LogInformation("New user registered successfully: {Email} ({Role})", user.Email, user.Role);

        return CreatedAtAction(nameof(GetCurrentUser), response);
    }

    /// <summary>
    /// Authenticate user and return JWT token
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var token = _tokenService.CreateToken(user, request.RememberMe);
        var expiry = _tokenService.GetTokenExpiryMinutes(request.RememberMe);

        var response = new AuthResponseDto
        {
            Token = token,
            TokenType = "Bearer",
            ExpiresInMinutes = expiry,
            User = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                Age = user.Age,
                Role = user.Role,
                City = user.City,
                State = user.State,
                CreatedAt = user.CreatedAt
            }
        };

        _logger.LogInformation("User logged in successfully: {Email}", user.Email);

        return Ok(response);
    }

    /// <summary>
    /// Get authenticated user's profile
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid authentication claims." });
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            MobileNumber = user.MobileNumber,
            Age = user.Age,
            Role = user.Role,
            City = user.City,
            State = user.State,
            CreatedAt = user.CreatedAt
        });
    }
}
