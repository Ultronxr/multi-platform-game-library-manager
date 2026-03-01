using GameLibrary.Api.Data;
using GameLibrary.Api.Data.Entities;
using GameLibrary.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameLibrary.Api.Services;

/// <summary>
/// 认证与用户管理服务实现。
/// </summary>
public sealed class AuthService(
    IDbContextFactory<GameLibraryDbContext> dbContextFactory,
    PasswordHashService passwordHashService,
    JwtTokenService jwtTokenService,
    AuthOptions authOptions,
    ILogger<AuthService> logger) : IAuthService
{
    /// <inheritdoc />
    public async Task<BootstrapStatusResponse> GetBootstrapStatusAsync(CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var hasAnyUser = await db.Users.AnyAsync(cancellationToken);
        // 仅当系统尚无用户且配置了 BootstrapToken 时，才允许初始化管理员。
        var bootstrapEnabled = !hasAnyUser && !string.IsNullOrWhiteSpace(authOptions.BootstrapToken);

        return new BootstrapStatusResponse(hasAnyUser, bootstrapEnabled);
    }

    /// <inheritdoc />
    public async Task<ServiceOperationResult> BootstrapAdminAsync(
        BootstrapAdminRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(authOptions.BootstrapToken))
        {
            return ServiceOperationResult.Failure(
                StatusCodes.Status400BadRequest,
                "Bootstrap token is not configured on server.");
        }

        if (!string.Equals(request.SetupToken?.Trim(), authOptions.BootstrapToken, StringComparison.Ordinal))
        {
            return ServiceOperationResult.Failure(StatusCodes.Status401Unauthorized, "Invalid setup token.");
        }

        if (!ValidateCredential(request.Username, request.Password, out var validationError))
        {
            return ServiceOperationResult.Failure(StatusCodes.Status400BadRequest, validationError);
        }

        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var hasUsers = await db.Users.AnyAsync(cancellationToken);
        if (hasUsers)
        {
            return ServiceOperationResult.Failure(
                StatusCodes.Status400BadRequest,
                "Bootstrap is only available when no users exist.");
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
        return ServiceOperationResult.Success(message: "Bootstrap admin created successfully.");
    }

    /// <inheritdoc />
    public async Task<ServiceOperationResult<AuthLoginResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return ServiceOperationResult<AuthLoginResponse>.Failure(
                StatusCodes.Status400BadRequest,
                "Username and password are required.");
        }

        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var username = request.Username.Trim();
        var user = await db.Users.FirstOrDefaultAsync(x => x.Username == username, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return ServiceOperationResult<AuthLoginResponse>.Failure(
                StatusCodes.Status401Unauthorized,
                "Invalid username or password.");
        }

        if (!passwordHashService.VerifyPassword(request.Password, user.PasswordHash))
        {
            return ServiceOperationResult<AuthLoginResponse>.Failure(
                StatusCodes.Status401Unauthorized,
                "Invalid username or password.");
        }

        // 登录成功后回写最近登录时间，便于审计与后续风控。
        user.LastLoginAtUtc = DateTime.UtcNow;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        var (token, expiresAtUtc) = jwtTokenService.CreateAccessToken(user);
        var response = new AuthLoginResponse(token, expiresAtUtc, user.Username, user.Role);
        return ServiceOperationResult<AuthLoginResponse>.Success(response);
    }

    /// <inheritdoc />
    public async Task<ServiceOperationResult> CreateUserAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        if (!ValidateCredential(request.Username, request.Password, out var validationError))
        {
            return ServiceOperationResult.Failure(StatusCodes.Status400BadRequest, validationError);
        }

        var role = NormalizeRole(request.Role);
        if (role is null)
        {
            return ServiceOperationResult.Failure(
                StatusCodes.Status400BadRequest,
                "Role must be `admin` or `user`.");
        }

        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var username = request.Username.Trim();
        var exists = await db.Users.AnyAsync(x => x.Username == username, cancellationToken);
        if (exists)
        {
            return ServiceOperationResult.Failure(StatusCodes.Status409Conflict, "Username already exists.");
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
        return ServiceOperationResult.Success(message: "User created.");
    }

    /// <inheritdoc />
    public ServiceOperationResult<CurrentUserResponse> GetCurrentUser(System.Security.Claims.ClaimsPrincipal principal)
    {
        var username = principal.Identity?.Name ?? string.Empty;
        var role = principal.Claims.FirstOrDefault(
            x => x.Type == System.Security.Claims.ClaimTypes.Role)?.Value ?? "user";

        return ServiceOperationResult<CurrentUserResponse>.Success(new CurrentUserResponse(username, role));
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
