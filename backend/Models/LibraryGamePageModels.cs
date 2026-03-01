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
/// 同组库存下的明细项（用于展开查看）。
/// </summary>
public sealed record LibraryGameGroupItem(
    string ExternalId,
    string? EpicAppName,
    DateTime SyncedAtUtc);

/// <summary>
/// 库存分页项（按同平台+同账号+同游戏聚合后展示）。
/// </summary>
public sealed record LibraryGameListItem(
    string GroupKey,
    string Title,
    GamePlatform Platform,
    string AccountName,
    string? AccountExternalId,
    DateTime SyncedAtUtc,
    int GroupItemCount,
    string? EpicAppName,
    IReadOnlyCollection<LibraryGameGroupItem> GroupItems);

/// <summary>
/// 库存分页查询响应。
/// </summary>
public sealed record LibraryGamesPageResponse(
    int PageNumber,
    int PageSize,
    int TotalCount,
    IReadOnlyCollection<LibraryGameListItem> Items);
