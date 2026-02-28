namespace GameLibrary.Api.Models;

/// <summary>
/// 已拥有游戏信息。
/// </summary>
public sealed record OwnedGame(
    string ExternalId,
    string Title,
    GamePlatform Platform,
    string AccountName,
    DateTime SyncedAtUtc);
