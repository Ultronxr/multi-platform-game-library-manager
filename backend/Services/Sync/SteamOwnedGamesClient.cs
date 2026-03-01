using System.Text.Json;
using GameLibrary.Api.Models;

namespace GameLibrary.Api.Services;

/// <summary>
/// Steam 已拥有游戏拉取客户端。
/// </summary>
public sealed class SteamOwnedGamesClient(
    HttpClient httpClient,
    ILogger<SteamOwnedGamesClient> logger)
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

        try
        {
            using var response = await httpClient.GetAsync(url, cancellationToken);
            var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogDebug(
                "Steam API 响应：Url={Url}; StatusCode={StatusCode}; ReasonPhrase={ReasonPhrase}; Body={Body}",
                url,
                (int)response.StatusCode,
                response.ReasonPhrase,
                rawContent);

            if (!response.IsSuccessStatusCode)
            {
                return SyncResult.Failure($"Steam API request failed: {(int)response.StatusCode} {response.ReasonPhrase}");
            }

            using var json = JsonDocument.Parse(rawContent);

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
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            logger.LogDebug(
                ex,
                "Steam API 请求异常：Url={Url}; Error={Error}",
                url,
                ex.Message);
            throw;
        }
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
