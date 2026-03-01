using System.Security.Claims;
using GameLibrary.Api.Models;

namespace GameLibrary.Api.Services;

/// <summary>
/// 认证与用户管理服务。
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// 查询是否允许初始化管理员。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>初始化状态。</returns>
    Task<BootstrapStatusResponse> GetBootstrapStatusAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 初始化首个管理员账号。
    /// </summary>
    /// <param name="request">初始化请求。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>操作结果。</returns>
    Task<ServiceOperationResult> BootstrapAdminAsync(
        BootstrapAdminRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// 用户登录。
    /// </summary>
    /// <param name="request">登录请求。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>带令牌的操作结果。</returns>
    Task<ServiceOperationResult<AuthLoginResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// 创建用户。
    /// </summary>
    /// <param name="request">创建用户请求。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>操作结果。</returns>
    Task<ServiceOperationResult> CreateUserAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// 获取当前用户信息。
    /// </summary>
    /// <param name="principal">当前认证主体。</param>
    /// <returns>当前用户信息结果。</returns>
    ServiceOperationResult<CurrentUserResponse> GetCurrentUser(ClaimsPrincipal principal);
}
