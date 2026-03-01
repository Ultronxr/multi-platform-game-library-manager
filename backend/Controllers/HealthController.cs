using GameLibrary.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameLibrary.Api.Controllers;

/// <summary>
/// 健康检查控制器。
/// </summary>
[ApiController]
[Route("api/health")]
[AllowAnonymous]
public sealed class HealthController : ControllerBase
{
    /// <summary>
    /// 获取服务健康状态。
    /// </summary>
    /// <returns>健康状态结果。</returns>
    [HttpGet]
    public IActionResult Get() =>
        Ok(new { status = "ok", time = Utc8DateTimeFormatter.Format(DateTime.UtcNow) });
}
