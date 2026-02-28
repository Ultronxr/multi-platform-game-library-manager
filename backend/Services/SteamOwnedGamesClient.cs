using System.Text.Json;
using GameLibrary.Api.Models;

namespace GameLibrary.Api.Services;

public sealed class SteamOwnedGamesClient(HttpClient httpClient)
{
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
