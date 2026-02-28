namespace GameLibrary.Api.Models;

/// <summary>
/// 跨平台重复游戏分组。
/// </summary>
public sealed record DuplicateGroup(
    string NormalizedTitle,
    IReadOnlyCollection<OwnedGame> Games);

/// <summary>
/// 游戏库聚合响应。
/// </summary>
public sealed record LibraryResponse(
    int TotalGames,
    int DuplicateGroups,
    IReadOnlyCollection<OwnedGame> Games,
    IReadOnlyCollection<DuplicateGroup> Duplicates);
