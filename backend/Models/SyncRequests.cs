namespace GameLibrary.Api.Models;

/// <summary>
/// Steam 同步请求体。
/// </summary>
public sealed record SteamSyncRequest(
    string SteamId,
    string? ApiKey,
    string? AccountName);

/// <summary>
/// Epic 同步请求体。
/// </summary>
public sealed record EpicSyncRequest(
    string AccessToken,
    string? AccountName);
