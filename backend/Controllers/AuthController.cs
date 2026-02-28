using GameLibrary.Api.Data;
using GameLibrary.Api.Data.Entities;
using GameLibrary.Api.Models;
using GameLibrary.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameLibrary.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IDbContextFactory<GameLibraryDbContext> dbContextFactory,
    PasswordHashService passwordHashService,
    JwtTokenService jwtTokenService,
    AuthOptions authOptions,
    ILogger<AuthController> logger) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("bootstrap-status")]
    public async Task<IActionResult> BootstrapStatus(CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var hasAnyUser = await db.Users.AnyAsync(cancellationToken);
        // 仅在系统尚无用户且服务端配置了 BootstrapToken 时允许初始化管理员。
        var bootstrapEnabled = !hasAnyUser && !string.IsNullOrWhiteSpace(authOptions.BootstrapToken);

        return Ok(new
        {
            hasAnyUser,
            bootstrapEnabled
        });
    }

    [AllowAnonymous]
    [HttpPost("bootstrap-admin")]
    public async Task<IActionResult> BootstrapAdmin(
        [FromBody] BootstrapAdminRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(authOptions.BootstrapToken))
        {
            return BadRequest(new { message = "Bootstrap token is not configured on server." });
        }

        if (!string.Equals(request.SetupToken?.Trim(), authOptions.BootstrapToken, StringComparison.Ordinal))
        {
            return Unauthorized(new { message = "Invalid setup token." });
        }

        if (!ValidateCredential(request.Username, request.Password, out var validationError))
        {
            return BadRequest(new { message = validationError });
        }

        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var hasUsers = await db.Users.AnyAsync(cancellationToken);
        if (hasUsers)
        {
            return BadRequest(new { message = "Bootstrap is only available when no users exist." });
        }

        var now = DateTime.UtcNow;
        var user = new AppUserEntity
        {
            Username = request.Username.Trim(),
            PasswordHash = passwordHashService.HashPassword(request.Password),
            Role = "admin",
            IsActive = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Bootstrap admin user created: {Username}", user.Username);
        return Ok(new { message = "Bootstrap admin created successfully." });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username and password are required." });
        }

        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var username = request.Username.Trim();
        var user = await db.Users.FirstOrDefaultAsync(x => x.Username == username, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        if (!passwordHashService.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        user.LastLoginAtUtc = DateTime.UtcNow;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        var (token, expiresAtUtc) = jwtTokenService.CreateAccessToken(user);
        return Ok(new AuthLoginResponse(token, expiresAtUtc, user.Username, user.Role));
    }

    [Authorize(Roles = "admin")]
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (!ValidateCredential(request.Username, request.Password, out var validationError))
        {
            return BadRequest(new { message = validationError });
        }

        var role = NormalizeRole(request.Role);
        if (role is null)
        {
            return BadRequest(new { message = "Role must be `admin` or `user`." });
        }

        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var username = request.Username.Trim();
        var exists = await db.Users.AnyAsync(x => x.Username == username, cancellationToken);
        if (exists)
        {
            return Conflict(new { message = "Username already exists." });
        }

        var now = DateTime.UtcNow;
        db.Users.Add(new AppUserEntity
        {
            Username = username,
            PasswordHash = passwordHashService.HashPassword(request.Password),
            Role = role,
            IsActive = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });

        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "User created." });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var username = User.Identity?.Name ?? string.Empty;
        var role = User.Claims.FirstOrDefault(x => x.Type == System.Security.Claims.ClaimTypes.Role)?.Value ?? "user";
        return Ok(new { username, role });
    }

    private static bool ValidateCredential(string? username, string? password, out string message)
    {
        message = string.Empty;
        if (string.IsNullOrWhiteSpace(username))
        {
            message = "Username is required.";
            return false;
        }

        if (username.Trim().Length < 3 || username.Trim().Length > 64)
        {
            message = "Username length must be between 3 and 64.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            message = "Password length must be at least 8.";
            return false;
        }

        return true;
    }

    private static string? NormalizeRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return "user";
        }

        return role.Trim().ToLowerInvariant() switch
        {
            "admin" => "admin",
            "user" => "user",
            _ => null
        };
    }
}
