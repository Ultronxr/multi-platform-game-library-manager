namespace GameLibrary.Api.Models;

public sealed record OwnedGame(
    string ExternalId,
    string Title,
    GamePlatform Platform,
    string AccountName,
    DateTime SyncedAtUtc);
