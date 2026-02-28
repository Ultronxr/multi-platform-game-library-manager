namespace GameLibrary.Api.Models;

public sealed record SteamSyncRequest(
    string SteamId,
    string? ApiKey,
    string? AccountName);

public sealed record EpicSyncRequest(
    string AccessToken,
    string? AccountName);
