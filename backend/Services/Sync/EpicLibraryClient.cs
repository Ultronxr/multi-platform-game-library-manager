using System.Text.Json;
using GameLibrary.Api.Models;

namespace GameLibrary.Api.Services;

/// <summary>
/// Epic 已拥有游戏拉取客户端。
/// </summary>
public sealed class EpicLibraryClient(
    HttpClient httpClient,
    ILogger<EpicLibraryClient> logger)
{
    /// <summary>
    /// 调用 Epic 库存接口获取账号已拥有游戏。
    /// </summary>
    /// <param name="accessToken">Epic Bearer Token。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>同步结果。</returns>
    public async Task<SyncResult> GetOwnedGamesAsync(string accessToken, CancellationToken cancellationToken)
    {
        const string url =
            "https://library-service.live.use1a.on.epicgames.com/library/api/public/items?includeMetadata=true";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogDebug(
                "Epic API 响应：Url={Url}; StatusCode={StatusCode}; ReasonPhrase={ReasonPhrase}; Body={Body}",
                url,
                (int)response.StatusCode,
                response.ReasonPhrase,
                rawContent);

            if (!response.IsSuccessStatusCode)
            {
                return SyncResult.Failure($"Epic library request failed: {(int)response.StatusCode} {response.ReasonPhrase}");
            }

            using var json = JsonDocument.Parse(rawContent);

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
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            logger.LogDebug(
                ex,
                "Epic API 请求异常：Url={Url}; Error={Error}",
                url,
                ex.Message);
            throw;
        }
    }

    private static IEnumerable<JsonElement> ExtractItems(JsonElement root)
    {
        // Epic 返回结构在不同场景可能为数组或 records/elements/items 包装对象，统一兜底提取。
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
            ReadString(item, "sandboxName")
            ?? ReadString(item, "title")
            ?? ReadString(item, "displayName")
            ?? ReadString(item, "appName");

        if (!string.IsNullOrWhiteSpace(directTitle))
        {
            return directTitle;
        }

        if (item.TryGetProperty("metadata", out var metadata) &&
            metadata.ValueKind == JsonValueKind.Object)
        {
            return ReadString(metadata, "sandboxName")
                ?? ReadString(metadata, "title")
                ?? ReadString(metadata, "displayName")
                ?? ReadString(metadata, "appName");
        }

        if (item.TryGetProperty("catalogItem", out var catalogItem) &&
            catalogItem.ValueKind == JsonValueKind.Object)
        {
            return ReadString(catalogItem, "sandboxName")
                ?? ReadString(catalogItem, "title")
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
