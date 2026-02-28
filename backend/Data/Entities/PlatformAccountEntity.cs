using GameLibrary.Api.Models;

namespace GameLibrary.Api.Data.Entities;

public sealed class PlatformAccountEntity
{
    public long Id { get; set; }
    public GamePlatform Platform { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string? ExternalAccountId { get; set; }
    public string CredentialType { get; set; } = string.Empty;
    public string CredentialValue { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? LastSyncedAtUtc { get; set; }
    public List<OwnedGameEntity> OwnedGames { get; set; } = [];
}
