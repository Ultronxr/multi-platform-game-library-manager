using GameLibrary.Api.Models;
using GameLibrary.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameLibrary.Api.Controllers;

/// <summary>
/// 认证与用户管理控制器。
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IAuthService authService) : ControllerBase
{
    /// <summary>
    /// 查询是否可执行首次管理员初始化。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>初始化状态。</returns>
    [AllowAnonymous]
    [HttpGet("bootstrap-status")]
    public async Task<IActionResult> BootstrapStatus(CancellationToken cancellationToken)
    {
        var status = await authService.GetBootstrapStatusAsync(cancellationToken);
        return Ok(status);
    }

    /// <summary>
    /// 初始化首个管理员账号。
    /// </summary>
    /// <param name="request">初始化请求。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>初始化结果。</returns>
    [AllowAnonymous]
    [HttpPost("bootstrap-admin")]
    public async Task<IActionResult> BootstrapAdmin(
        [FromBody] BootstrapAdminRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.BootstrapAdminAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// 用户登录并签发 JWT 访问令牌。
    /// </summary>
    /// <param name="request">登录请求。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>登录结果。</returns>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// 创建用户（仅管理员可调用）。
    /// </summary>
    /// <param name="request">创建用户请求。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>创建结果。</returns>
    [Authorize(Roles = "admin")]
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.CreateUserAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// 获取当前登录用户信息。
    /// </summary>
    /// <returns>用户名与角色信息。</returns>
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var result = authService.GetCurrentUser(User);
        return Ok(result.Data);
    }
}
