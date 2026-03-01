using GameLibrary.Api.Models;

namespace GameLibrary.Api.Data.Entities;

/// <summary>
/// 平台账号实体。
/// </summary>
public sealed class PlatformAccountEntity
{
    /// <summary>
    /// 账号记录主键。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 所属游戏平台。
    /// </summary>
    public GamePlatform Platform { get; set; }

    /// <summary>
    /// 平台账号显示名称。
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// 平台侧账号 ID（可选）。
    /// </summary>
    public string? ExternalAccountId { get; set; }

    /// <summary>
    /// 凭证类型（如 steam_api_key）。
    /// </summary>
    public string CredentialType { get; set; } = string.Empty;

    /// <summary>
    /// 凭证原文值。
    /// </summary>
    public string CredentialValue { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间（UTC+8）。
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// 更新时间（UTC+8）。
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }

    /// <summary>
    /// 最近同步时间（UTC+8）。
    /// </summary>
    public DateTime? LastSyncedAtUtc { get; set; }

    /// <summary>
    /// 该账号下的游戏集合。
    /// </summary>
    public List<OwnedGameEntity> OwnedGames { get; set; } = [];
}
