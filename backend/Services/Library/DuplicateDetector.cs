using GameLibrary.Api.Models;

namespace GameLibrary.Api.Services;

/// <summary>
/// 跨平台重复游戏检测器。
/// </summary>
public sealed class DuplicateDetector
{
    /// <summary>
    /// 按归一化标题识别跨平台重复组。
    /// </summary>
    /// <param name="games">待检测游戏集合。</param>
    /// <returns>重复组集合。</returns>
    public IReadOnlyCollection<DuplicateGroup> FindCrossPlatformDuplicates(
        IReadOnlyCollection<OwnedGame> games)
    {
        // 仅保留至少出现在两个不同平台的标题分组，避免同平台重复记录误报。
        return games
            .Where(x => !string.IsNullOrWhiteSpace(x.Title))
            .GroupBy(x => TitleNormalizer.Normalize(x.Title), StringComparer.Ordinal)
            .Where(g => !string.IsNullOrWhiteSpace(g.Key))
            .Where(g => g.Select(x => x.Platform).Distinct().Count() > 1)
            .Select(g => new DuplicateGroup(g.Key, g.ToList()))
            .OrderBy(g => g.NormalizedTitle, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
