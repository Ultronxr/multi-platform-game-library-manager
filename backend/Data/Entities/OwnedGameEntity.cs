using GameLibrary.Api.Models;

namespace GameLibrary.Api.Data.Entities;

public sealed class OwnedGameEntity
{
    public long Id { get; set; }
    public long AccountId { get; set; }
    public PlatformAccountEntity Account { get; set; } = null!;
    public GamePlatform Platform { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string ExternalGameId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string NormalizedTitle { get; set; } = string.Empty;
    public DateTime SyncedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
