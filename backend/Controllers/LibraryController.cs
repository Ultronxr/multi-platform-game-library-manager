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
    IGameLibraryStore store,
    DuplicateDetector duplicateDetector) : ControllerBase
{
    /// <summary>
    /// 查询已保存的平台账号列表。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>账号集合。</returns>
    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccounts(CancellationToken cancellationToken)
    {
        var accounts = await store.GetAllAccountsAsync(cancellationToken);
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
        // 统一在服务端排序，避免不同客户端对大小写排序规则不一致。
        var allGames = (await store.GetAllGamesAsync(cancellationToken))
            .OrderBy(x => x.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var duplicates = duplicateDetector.FindCrossPlatformDuplicates(allGames);

        return Ok(new LibraryResponse(
            allGames.Count,
            duplicates.Count,
            allGames,
            duplicates));
    }
}
