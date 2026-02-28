using System.Text.Json;
using GameLibrary.Api.Models;

namespace GameLibrary.Api.Services;

/// <summary>
/// Steam 已拥有游戏拉取客户端。
/// </summary>
public sealed class SteamOwnedGamesClient(HttpClient httpClient)
{
    /// <summary>
    /// 调用 Steam API 获取账号已拥有游戏。
    /// </summary>
    /// <param name="apiKey">Steam Web API Key。</param>
    /// <param name="steamId">SteamID64。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>同步结果。</returns>
    public async Task<SyncResult> GetOwnedGamesAsync(string apiKey, string steamId, CancellationToken cancellationToken)
    {
        var url =
            $"https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/?key={Uri.EscapeDataString(apiKey)}&steamid={Uri.EscapeDataString(steamId)}&include_appinfo=true&include_played_free_games=true";

        using var response = await httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return SyncResult.Failure($"Steam API request failed: {(int)response.StatusCode} {response.ReasonPhrase}");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (!json.RootElement.TryGetProperty("response", out var responseNode))
        {
            return SyncResult.Failure("Steam API response is invalid: missing `response`.");
        }

        if (!responseNode.TryGetProperty("games", out var gamesNode) ||
            gamesNode.ValueKind != JsonValueKind.Array)
        {
            // Steam 在账号无库存或隐私受限时可能不返回 games 数组，按空库处理。
            return SyncResult.Success([]);
        }

        var games = new List<OwnedGame>();
        foreach (var game in gamesNode.EnumerateArray())
        {
            var title = ReadString(game, "name");
            if (string.IsNullOrWhiteSpace(title))
            {
                continue;
            }

            var externalId = ReadNumberAsString(game, "appid") ?? title;
            games.Add(new OwnedGame(externalId, title, GamePlatform.Steam, string.Empty, DateTime.UtcNow));
        }

        return SyncResult.Success(games);
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    private static string? ReadNumberAsString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Number => value.ToString(),
            JsonValueKind.String => value.GetString(),
            _ => null
        };
    }
}
