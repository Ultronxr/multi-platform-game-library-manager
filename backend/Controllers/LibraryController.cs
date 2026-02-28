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
    ILibraryQueryService libraryQueryService) : ControllerBase
{
    /// <summary>
    /// 查询已保存的平台账号列表。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>账号集合。</returns>
    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccounts(CancellationToken cancellationToken)
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
    public async Task<IActionResult> GetLibrary(CancellationToken cancellationToken)
    {
        var library = await libraryQueryService.GetLibraryAsync(cancellationToken);
        return Ok(library);
    }
}
