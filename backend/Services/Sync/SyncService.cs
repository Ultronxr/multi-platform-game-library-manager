using GameLibrary.Api.Models;

namespace GameLibrary.Api.Services;

/// <summary>
/// 平台库存同步服务实现。
/// </summary>
public sealed class SyncService(
    IConfiguration configuration,
    SteamOwnedGamesClient steamClient,
    EpicLibraryClient epicClient,
    IGameLibraryStore store,
    ILogger<SyncService> logger) : ISyncService
{
    /// <inheritdoc />
    public async Task<ServiceOperationResult<int>> SyncSteamAsync(
        SteamSyncRequest request,
        CancellationToken cancellationToken)
    {
        var apiKey = string.IsNullOrWhiteSpace(request.ApiKey)
            ? configuration["Steam:ApiKey"]
            : request.ApiKey;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return ServiceOperationResult<int>.Failure(
                StatusCodes.Status400BadRequest,
                "Steam API Key is required.");
        }

        if (string.IsNullOrWhiteSpace(request.SteamId))
        {
            return ServiceOperationResult<int>.Failure(
                StatusCodes.Status400BadRequest,
                "SteamId is required.");
        }

        var steamId = request.SteamId.Trim();
        logger.LogInformation("Starting Steam sync for SteamId {SteamId}", steamId);

        var result = await steamClient.GetOwnedGamesAsync(apiKey, steamId, cancellationToken);
        if (!result.IsSuccess)
        {
            logger.LogWarning("Steam sync failed for SteamId {SteamId}: {Reason}", steamId, result.ErrorMessage);
            return ServiceOperationResult<int>.Failure(
                StatusCodes.Status400BadRequest,
                result.ErrorMessage ?? "Steam sync failed.");
        }

        var steamAccountName = string.IsNullOrWhiteSpace(request.AccountName)
            ? steamId
            : request.AccountName.Trim();

        // 保存时采用账号维度的全量覆盖策略，确保库存与本次同步结果一致。
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

        return ServiceOperationResult<int>.Success(result.Games.Count);
    }

    /// <inheritdoc />
    public async Task<ServiceOperationResult<int>> SyncEpicAsync(
        EpicSyncRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AccessToken))
        {
            return ServiceOperationResult<int>.Failure(
                StatusCodes.Status400BadRequest,
                "Epic access token is required.");
        }

        logger.LogInformation(
            "Starting Epic sync for account alias {AccountAlias}",
            request.AccountName ?? "EpicAccount");

        var result = await epicClient.GetOwnedGamesAsync(request.AccessToken, cancellationToken);
        if (!result.IsSuccess)
        {
            logger.LogWarning(
                "Epic sync failed for account alias {AccountAlias}: {Reason}",
                request.AccountName ?? "EpicAccount",
                result.ErrorMessage);
            return ServiceOperationResult<int>.Failure(
                StatusCodes.Status400BadRequest,
                result.ErrorMessage ?? "Epic sync failed.");
        }

        var epicAccountName = string.IsNullOrWhiteSpace(request.AccountName)
            ? "EpicAccount"
            : request.AccountName.Trim();

        // Epic 无稳定 externalAccountId，当前以账号别名作为展示维度。
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

        return ServiceOperationResult<int>.Success(result.Games.Count);
    }
}
