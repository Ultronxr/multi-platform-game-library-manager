using GameLibrary.Api.Models;
using GameLibrary.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameLibrary.Api.Controllers;

/// <summary>
/// 平台库存同步控制器。
/// </summary>
[ApiController]
[Route("api/sync")]
[Authorize]
public sealed class SyncController(
    ISyncService syncService) : ControllerBase
{
    /// <summary>
    /// 同步 Steam 库存。
    /// </summary>
    /// <param name="request">同步请求。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>同步结果。</returns>
    [HttpPost("steam")]
    public async Task<IActionResult> SyncSteam([FromBody] SteamSyncRequest request, CancellationToken cancellationToken)
    {
        var result = await syncService.SyncSteamAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(new { syncedCount = result.Data });
    }

    /// <summary>
    /// 同步 Epic 库存。
    /// </summary>
    /// <param name="request">同步请求。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>同步结果。</returns>
    [HttpPost("epic")]
    public async Task<IActionResult> SyncEpic([FromBody] EpicSyncRequest request, CancellationToken cancellationToken)
    {
        var result = await syncService.SyncEpicAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(new { syncedCount = result.Data });
    }
}
