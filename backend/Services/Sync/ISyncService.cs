using GameLibrary.Api.Models;

namespace GameLibrary.Api.Services;

/// <summary>
/// 平台库存同步服务。
/// </summary>
public interface ISyncService
{
    /// <summary>
    /// 同步 Steam 库存。
    /// </summary>
    /// <param name="request">同步请求。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>同步结果（返回同步数量）。</returns>
    Task<ServiceOperationResult<int>> SyncSteamAsync(
        SteamSyncRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// 同步 Epic 库存。
    /// </summary>
    /// <param name="request">同步请求。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>同步结果（返回同步数量）。</returns>
    Task<ServiceOperationResult<int>> SyncEpicAsync(
        EpicSyncRequest request,
        CancellationToken cancellationToken);
}
