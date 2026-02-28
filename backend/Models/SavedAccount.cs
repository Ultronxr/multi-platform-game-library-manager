namespace GameLibrary.Api.Models;

/// <summary>
/// 已保存的平台账号信息。
/// </summary>
public sealed record SavedAccount(
    long Id,
    GamePlatform Platform,
    string AccountName,
    string? ExternalAccountId,
    string CredentialType,
    string CredentialPreview,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? LastSyncedAtUtc);
