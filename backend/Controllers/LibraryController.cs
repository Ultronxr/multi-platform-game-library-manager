using GameLibrary.Api.Models;
using GameLibrary.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameLibrary.Api.Controllers;

/// <summary>
/// 游戏库查询控制器。
/// </summary>
[ApiController]
[Route("api")]
[Authorize]
public sealed class LibraryController(
    ILibraryQueryService libraryQueryService,
    IAccountManagementService accountManagementService) : ControllerBase
{
    /// <summary>
    /// 查询已保存的平台账号列表。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>账号集合。</returns>
    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccountsAsync(CancellationToken cancellationToken)
    {
        var accounts = await libraryQueryService.GetAccountsAsync(cancellationToken);
        return Ok(accounts);
    }

    /// <summary>
    /// 查询完整游戏库及重复组信息。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>库存聚合结果。</returns>
    [HttpGet("library")]
    public async Task<IActionResult> GetLibraryAsync(CancellationToken cancellationToken)
    {
        var library = await libraryQueryService.GetLibraryAsync(cancellationToken);
        return Ok(library);
    }

    /// <summary>
    /// 使用已保存凭证重新拉取指定账号的库存。
    /// </summary>
    /// <param name="accountId">账号主键。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>同步结果。</returns>
    [HttpPost("accounts/{accountId:long}/resync")]
    public async Task<IActionResult> ResyncAccountAsync(long accountId, CancellationToken cancellationToken)
    {
        var result = await accountManagementService.ResyncSavedAccountAsync(accountId, cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(new { syncedCount = result.Data });
    }

    /// <summary>
    /// 更新指定账号信息。
    /// </summary>
    /// <param name="accountId">账号主键。</param>
    /// <param name="request">更新请求。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>更新结果。</returns>
    [HttpPut("accounts/{accountId:long}")]
    public async Task<IActionResult> UpdateAccountAsync(
        long accountId,
        [FromBody] UpdateSavedAccountRequest request,
        CancellationToken cancellationToken)
    {
        var result = await accountManagementService.UpdateSavedAccountAsync(accountId, request, cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// 删除指定账号及关联库存。
    /// </summary>
    /// <param name="accountId">账号主键。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>删除结果。</returns>
    [HttpDelete("accounts/{accountId:long}")]
    public async Task<IActionResult> DeleteAccountAsync(long accountId, CancellationToken cancellationToken)
    {
        var result = await accountManagementService.DeleteSavedAccountAsync(accountId, cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(new { message = result.Message });
    }
}
