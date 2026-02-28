using System.Text.Json;
using GameLibrary.Api.Models;

namespace GameLibrary.Api.Services;

public sealed class EpicLibraryClient(HttpClient httpClient)
{
    public async Task<SyncResult> GetOwnedGamesAsync(string accessToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "https://library-service.live.use1a.on.epicgames.com/library/api/public/items?includeMetadata=true");
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return SyncResult.Failure($"Epic library request failed: {(int)response.StatusCode} {response.ReasonPhrase}");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var items = ExtractItems(json.RootElement);
        var games = new List<OwnedGame>();
        foreach (var item in items)
        {
            var title = ResolveTitle(item);
            if (string.IsNullOrWhiteSpace(title))
            {
                continue;
            }

            var externalId = ResolveId(item) ?? title;
            games.Add(new OwnedGame(externalId, title, GamePlatform.Epic, string.Empty, DateTime.UtcNow));
        }

        return SyncResult.Success(games);
    }

    private static IEnumerable<JsonElement> ExtractItems(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            return root.EnumerateArray().ToArray();
        }

        if (root.ValueKind != JsonValueKind.Object)
        {
            return [];
        }

        if (TryReadArray(root, "records", out var records))
        {
            return records;
        }

        if (TryReadArray(root, "elements", out var elements))
        {
            return elements;
        }

        if (TryReadArray(root, "items", out var items))
        {
            return items;
        }

        return [];
    }

    private static bool TryReadArray(JsonElement parent, string propertyName, out JsonElement[] items)
    {
        items = [];
        if (!parent.TryGetProperty(propertyName, out var node) || node.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        items = node.EnumerateArray().ToArray();
        return true;
    }

    private static string? ResolveId(JsonElement item) =>
        ReadString(item, "catalogItemId")
        ?? ReadString(item, "appName")
        ?? ReadString(item, "id")
        ?? ReadString(item, "artifactId");

    private static string? ResolveTitle(JsonElement item)
    {
        var directTitle =
            ReadString(item, "title")
            ?? ReadString(item, "displayName")
            ?? ReadString(item, "appName");

        if (!string.IsNullOrWhiteSpace(directTitle))
        {
            return directTitle;
        }

        if (item.TryGetProperty("metadata", out var metadata) &&
            metadata.ValueKind == JsonValueKind.Object)
        {
            return ReadString(metadata, "title")
                ?? ReadString(metadata, "displayName")
                ?? ReadString(metadata, "appName");
        }

        if (item.TryGetProperty("catalogItem", out var catalogItem) &&
            catalogItem.ValueKind == JsonValueKind.Object)
        {
            return ReadString(catalogItem, "title")
                ?? ReadString(catalogItem, "name");
        }

        return null;
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var node))
        {
            return null;
        }

        return node.ValueKind switch
        {
            JsonValueKind.String => node.GetString(),
            JsonValueKind.Number => node.ToString(),
            _ => null
        };
    }
}
