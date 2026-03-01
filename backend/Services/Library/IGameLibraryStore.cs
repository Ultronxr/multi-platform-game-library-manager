using GameLibrary.Api.Models;

namespace GameLibrary.Api.Services;

/// <summary>
/// 游戏库存存储抽象。
/// </summary>
public interface IGameLibraryStore
{
    /// <summary>
    /// 保存平台账号及对应游戏列表。
    /// </summary>
    /// <param name="platform">游戏平台。</param>
    /// <param name="accountName">账号名称。</param>
    /// <param name="externalAccountId">平台侧账号标识。</param>
    /// <param name="credentialType">凭证类型。</param>
    /// <param name="credentialValue">凭证值。</param>
    /// <param name="games">游戏列表。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>异步任务。</returns>
    Task SaveAccountAndGamesAsync(
        GamePlatform platform,
        string accountName,
        string? externalAccountId,
        string credentialType,
        string credentialValue,
        IReadOnlyCollection<OwnedGame> games,
        CancellationToken cancellationToken);

    /// <summary>
    /// 查询全部游戏库存。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>游戏集合。</returns>
    Task<IReadOnlyCollection<OwnedGame>> GetAllGamesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 查询全部平台账号。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>账号集合。</returns>
    Task<IReadOnlyCollection<SavedAccount>> GetAllAccountsAsync(CancellationToken cancellationToken);
}
