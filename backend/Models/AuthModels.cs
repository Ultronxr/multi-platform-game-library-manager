namespace GameLibrary.Api.Models;

/// <summary>
/// 用户登录请求体。
/// </summary>
public sealed record LoginRequest(
    string Username,
    string Password);

/// <summary>
/// 初始化管理员请求体。
/// </summary>
public sealed record BootstrapAdminRequest(
    string SetupToken,
    string Username,
    string Password);

/// <summary>
/// 创建用户请求体。
/// </summary>
public sealed record CreateUserRequest(
    string Username,
    string Password,
    string? Role);

/// <summary>
/// 登录成功后返回的鉴权信息。
/// </summary>
public sealed record AuthLoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string Username,
    string Role);

/// <summary>
/// 当前登录用户信息。
/// </summary>
public sealed record CurrentUserResponse(
    string Username,
    string Role);

/// <summary>
/// 初始化管理员可用状态。
/// </summary>
public sealed record BootstrapStatusResponse(
    bool HasAnyUser,
    bool BootstrapEnabled);
