namespace GameLibrary.Api.Services;

/// <summary>
/// 鉴权配置项。
/// </summary>
public sealed class AuthOptions
{
    /// <summary>
    /// JWT 签发者。
    /// </summary>
    public string Issuer { get; init; } = string.Empty;

    /// <summary>
    /// JWT 受众。
    /// </summary>
    public string Audience { get; init; } = string.Empty;

    /// <summary>
    /// JWT 签名密钥。
    /// </summary>
    public string SigningKey { get; init; } = string.Empty;

    /// <summary>
    /// 访问令牌有效期（分钟）。
    /// </summary>
    public int AccessTokenMinutes { get; init; } = 120;

    /// <summary>
    /// 首次初始化管理员口令。
    /// </summary>
    public string BootstrapToken { get; init; } = string.Empty;
}
