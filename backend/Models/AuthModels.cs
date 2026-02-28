namespace GameLibrary.Api.Models;

public sealed record LoginRequest(
    string Username,
    string Password);

public sealed record BootstrapAdminRequest(
    string SetupToken,
    string Username,
    string Password);

public sealed record CreateUserRequest(
    string Username,
    string Password,
    string? Role);

public sealed record AuthLoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string Username,
    string Role);
