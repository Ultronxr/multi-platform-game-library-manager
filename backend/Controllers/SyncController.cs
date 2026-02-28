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
    IConfiguration configuration,
    SteamOwnedGamesClient steamClient,
    EpicLibraryClient epicClient,
    IGameLibraryStore store,
    ILogger<SyncController> logger) : ControllerBase
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
        var apiKey = string.IsNullOrWhiteSpace(request.ApiKey)
            ? configuration["Steam:ApiKey"]
            : request.ApiKey;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return BadRequest(new { message = "Steam API Key is required." });
        }

        if (string.IsNullOrWhiteSpace(request.SteamId))
        {
            return BadRequest(new { message = "SteamId is required." });
        }

        var steamId = request.SteamId.Trim();
        logger.LogInformation("Starting Steam sync for SteamId {SteamId}", steamId);

        var result = await steamClient.GetOwnedGamesAsync(apiKey, steamId, cancellationToken);
        if (!result.IsSuccess)
        {
            logger.LogWarning("Steam sync failed for SteamId {SteamId}: {Reason}", steamId, result.ErrorMessage);
            return BadRequest(new { message = result.ErrorMessage });
        }

        var steamAccountName = string.IsNullOrWhiteSpace(request.AccountName)
            ? steamId
            : request.AccountName.Trim();

        await store.SaveAccountAndGamesAsync(
            GamePlatform.Steam,
            steamAccountName,
            steamId,
            "steam_api_key",
            apiKey.Trim(),
            result.Games,
            cancellationToken);

        logger.LogInformation(
            "Steam sync finished for account {AccountName}. Synced {GameCount} games.",
            steamAccountName,
            result.Games.Count);

        return Ok(new { syncedCount = result.Games.Count });
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
        if (string.IsNullOrWhiteSpace(request.AccessToken))
        {
            return BadRequest(new { message = "Epic access token is required." });
        }

        logger.LogInformation("Starting Epic sync for account alias {AccountAlias}", request.AccountName ?? "EpicAccount");

        var result = await epicClient.GetOwnedGamesAsync(request.AccessToken, cancellationToken);
        if (!result.IsSuccess)
        {
            logger.LogWarning("Epic sync failed for account alias {AccountAlias}: {Reason}", request.AccountName ?? "EpicAccount", result.ErrorMessage);
            return BadRequest(new { message = result.ErrorMessage });
        }

        var epicAccountName = string.IsNullOrWhiteSpace(request.AccountName)
            ? "EpicAccount"
            : request.AccountName.Trim();

        await store.SaveAccountAndGamesAsync(
            GamePlatform.Epic,
            epicAccountName,
            null,
            "epic_access_token",
            request.AccessToken.Trim(),
            result.Games,
            cancellationToken);

        logger.LogInformation(
            "Epic sync finished for account {AccountName}. Synced {GameCount} games.",
            epicAccountName,
            result.Games.Count);

        return Ok(new { syncedCount = result.Games.Count });
    }
}
