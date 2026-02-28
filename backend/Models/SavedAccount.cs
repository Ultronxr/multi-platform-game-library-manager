namespace GameLibrary.Api.Models;

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
