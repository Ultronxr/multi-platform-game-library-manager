namespace GameLibrary.Api.Models;

/// <summary>
/// 已保存账号更新请求体。
/// </summary>
public sealed record UpdateSavedAccountRequest(
    string? AccountName,
    string? ExternalAccountId,
    string? CredentialValue);
