using GameLibrary.Api.Models;

namespace GameLibrary.Api.Data.Entities;

/// <summary>
/// 已拥有游戏实体。
/// </summary>
public sealed class OwnedGameEntity
{
    /// <summary>
    /// 游戏记录主键。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 所属平台账号主键。
    /// </summary>
    public long AccountId { get; set; }

    /// <summary>
    /// 所属平台账号导航属性。
    /// </summary>
    public PlatformAccountEntity Account { get; set; } = null!;

    /// <summary>
    /// 游戏来源平台。
    /// </summary>
    public GamePlatform Platform { get; set; }

    /// <summary>
    /// 账号名称快照。
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// 平台侧游戏 ID。
    /// </summary>
    public string ExternalGameId { get; set; } = string.Empty;

    /// <summary>
    /// 游戏原始标题。
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 归一化标题（用于重复检测）。
    /// </summary>
    public string NormalizedTitle { get; set; } = string.Empty;

    /// <summary>
    /// 同步时间（UTC+8）。
    /// </summary>
    public DateTime SyncedAtUtc { get; set; }

    /// <summary>
    /// 创建时间（UTC+8）。
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }
}
