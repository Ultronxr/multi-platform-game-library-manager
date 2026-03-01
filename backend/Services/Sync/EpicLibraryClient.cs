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
    private const string EpicLibraryItemsUrl =
        "https://library-service.live.use1a.on.epicgames.com/library/api/public/items";

    /// <summary>
    /// 调用 Epic 库存接口获取账号已拥有游戏。
    /// </summary>
    /// <param name="accessToken">Epic Bearer Token。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>同步结果。</returns>
    public async Task<SyncResult> GetOwnedGamesAsync(string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            var pageNumber = 1;
            string? nextCursor = null;
            string? stateToken = null;
            var games = new List<OwnedGame>();
            var uniqueExternalIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            while (true)
            {
                var requestUrl = BuildItemsUrl(nextCursor, stateToken);
                using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                using var response = await httpClient.SendAsync(request, cancellationToken);
                var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogDebug(
                    "Epic API 响应：Page={Page}; Url={Url}; StatusCode={StatusCode}; ReasonPhrase={ReasonPhrase}; Body={Body}",
                    pageNumber,
                    requestUrl,
                    (int)response.StatusCode,
                    response.ReasonPhrase,
                    rawContent);

                if (!response.IsSuccessStatusCode)
                {
                    return SyncResult.Failure($"Epic library request failed: {(int)response.StatusCode} {response.ReasonPhrase}");
                }

                using var json = JsonDocument.Parse(rawContent);
                var items = ExtractItems(json.RootElement);
                var addedCount = 0;
                foreach (var item in items)
                {
                    var title = ResolveTitle(item);
                    if (string.IsNullOrWhiteSpace(title))
                    {
                        continue;
                    }

                    var externalId = ResolveId(item) ?? title;
                    if (!uniqueExternalIds.Add(externalId))
                    {
                        continue;
                    }

                    var epicAppName = ResolveEpicAppName(item);
                    games.Add(new OwnedGame(
                        externalId,
                        title,
                        GamePlatform.Epic,
                        string.Empty,
                        DateTime.UtcNow,
                        epicAppName));
                    addedCount++;
                }

                var responseStateToken = ResolveStateToken(json.RootElement);
                var responseNextCursor = ResolveNextCursor(json.RootElement);
                logger.LogDebug(
                    "Epic API 分页状态：Page={Page}; AddedCount={AddedCount}; TotalCount={TotalCount}; NextCursor={NextCursor}; StateToken={StateToken}",
                    pageNumber,
                    addedCount,
                    games.Count,
                    responseNextCursor ?? string.Empty,
                    responseStateToken ?? string.Empty);

                if (string.IsNullOrWhiteSpace(responseNextCursor))
                {
                    break;
                }

                if (string.Equals(responseNextCursor, nextCursor, StringComparison.Ordinal))
                {
                    logger.LogWarning(
                        "Epic API 分页游标未推进，提前停止以避免死循环。Page={Page}; Cursor={Cursor}",
                        pageNumber,
                        responseNextCursor);
                    break;
                }

                nextCursor = responseNextCursor;
                stateToken = string.IsNullOrWhiteSpace(responseStateToken) ? stateToken : responseStateToken;
                pageNumber++;
            }            

            return SyncResult.Success(games);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            logger.LogDebug(
                ex,
                "Epic API 请求异常：Url={Url}; Error={Error}",
                EpicLibraryItemsUrl,
                ex.Message);
            throw;
        }
    }

    private static string BuildItemsUrl(string? cursor, string? stateToken)
    {
        var queryItems = new List<string> { "includeMetadata=true" };
        if (!string.IsNullOrWhiteSpace(cursor))
        {
            queryItems.Add($"cursor={Uri.EscapeDataString(cursor)}");
        }

        if (!string.IsNullOrWhiteSpace(stateToken))
        {
            queryItems.Add($"stateToken={Uri.EscapeDataString(stateToken)}");
        }

        return $"{EpicLibraryItemsUrl}?{string.Join("&", queryItems)}";
    }

    private static string? ResolveNextCursor(JsonElement root)
    {
        if (!TryReadResponseMetadata(root, out var responseMetadata))
        {
            return null;
        }

        return ReadString(responseMetadata, "nextCursor");
    }

    private static string? ResolveStateToken(JsonElement root)
    {
        if (!TryReadResponseMetadata(root, out var responseMetadata))
        {
            return null;
        }

        return ReadString(responseMetadata, "stateToken");
    }

    private static bool TryReadResponseMetadata(JsonElement root, out JsonElement responseMetadata)
    {
        responseMetadata = default;
        if (root.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (!root.TryGetProperty("responseMetadata", out responseMetadata) ||
            responseMetadata.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        return true;
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

    private static string? ResolveEpicAppName(JsonElement item)
    {
        var directAppName = ReadString(item, "appName");
        if (!string.IsNullOrWhiteSpace(directAppName))
        {
            return directAppName;
        }

        if (item.TryGetProperty("metadata", out var metadata) &&
            metadata.ValueKind == JsonValueKind.Object)
        {
            return ReadString(metadata, "appName");
        }

        if (item.TryGetProperty("catalogItem", out var catalogItem) &&
            catalogItem.ValueKind == JsonValueKind.Object)
        {
            return ReadString(catalogItem, "appName");
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
