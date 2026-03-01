namespace GameLibrary.Api.Data.Entities;

/// <summary>
/// 应用用户实体。
/// </summary>
public sealed class AppUserEntity
{
    /// <summary>
    /// 用户主键。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 用户名（系统内唯一）。
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 密码哈希值。
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 用户角色（admin/user）。
    /// </summary>
    public string Role { get; set; } = "user";

    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 创建时间（UTC+8）。
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// 更新时间（UTC+8）。
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }

    /// <summary>
    /// 最近登录时间（UTC+8）。
    /// </summary>
    public DateTime? LastLoginAtUtc { get; set; }
}
