using GameLibrary.Api.Models;

namespace GameLibrary.Api.Services;

/// <summary>
/// 平台同步结果。
/// </summary>
public sealed class SyncResult
{
    /// <summary>
    /// 是否成功。
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// 失败时的错误信息。
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// 同步到的游戏集合。
    /// </summary>
    public IReadOnlyCollection<OwnedGame> Games { get; init; } = [];

    /// <summary>
    /// 创建成功结果。
    /// </summary>
    /// <param name="games">已同步游戏集合。</param>
    /// <returns>成功结果对象。</returns>
    public static SyncResult Success(IReadOnlyCollection<OwnedGame> games) =>
        new() { IsSuccess = true, Games = games };

    /// <summary>
    /// 创建失败结果。
    /// </summary>
    /// <param name="message">失败原因。</param>
    /// <returns>失败结果对象。</returns>
    public static SyncResult Failure(string message) =>
        new() { IsSuccess = false, ErrorMessage = message };
}
