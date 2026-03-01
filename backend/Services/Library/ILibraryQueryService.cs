using GameLibrary.Api.Models;

namespace GameLibrary.Api.Services;

/// <summary>
/// 库存查询服务。
/// </summary>
public interface ILibraryQueryService
{
    /// <summary>
    /// 查询已保存账号列表。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>账号集合。</returns>
    Task<IReadOnlyCollection<SavedAccount>> GetAccountsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 查询库存聚合信息。
    /// </summary>
    /// <param name="includeGames">是否包含全部库存明细。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>库存响应。</returns>
    Task<LibraryResponse> GetLibraryAsync(bool includeGames, CancellationToken cancellationToken);

    /// <summary>
    /// 分页查询库存明细。
    /// </summary>
    /// <param name="request">分页与筛选参数。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>分页结果。</returns>
    Task<LibraryGamesPageResponse> GetLibraryGamesPageAsync(
        LibraryGamesQueryRequest request,
        CancellationToken cancellationToken);
}
