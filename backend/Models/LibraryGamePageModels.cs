namespace GameLibrary.Api.Models;

/// <summary>
/// 库存分页查询请求参数。
/// </summary>
public sealed record LibraryGamesQueryRequest(
    string? GameTitle,
    GamePlatform? Platform,
    string? AccountName,
    string? AccountExternalId,
    int PageNumber = 1,
    int PageSize = 20);

/// <summary>
/// 库存分页项。
/// </summary>
public sealed record LibraryGameListItem(
    string ExternalId,
    string Title,
    GamePlatform Platform,
    string AccountName,
    string? AccountExternalId,
    DateTime SyncedAtUtc);

/// <summary>
/// 库存分页查询响应。
/// </summary>
public sealed record LibraryGamesPageResponse(
    int PageNumber,
    int PageSize,
    int TotalCount,
    IReadOnlyCollection<LibraryGameListItem> Items);
