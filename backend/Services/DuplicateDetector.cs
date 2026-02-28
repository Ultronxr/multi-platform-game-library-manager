using GameLibrary.Api.Models;

namespace GameLibrary.Api.Services;

public sealed class DuplicateDetector
{
    public IReadOnlyCollection<DuplicateGroup> FindCrossPlatformDuplicates(
        IReadOnlyCollection<OwnedGame> games)
    {
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
