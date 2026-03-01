using GameLibrary.Api.Models;

namespace GameLibrary.Api.Services;

/// <summary>
/// 库存查询服务实现。
/// </summary>
public sealed class LibraryQueryService(
    IGameLibraryStore store,
    DuplicateDetector duplicateDetector) : ILibraryQueryService
{
    /// <inheritdoc />
    public async Task<IReadOnlyCollection<SavedAccount>> GetAccountsAsync(CancellationToken cancellationToken) =>
        await store.GetAllAccountsAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<LibraryResponse> GetLibraryAsync(CancellationToken cancellationToken)
    {
        var allGames = (await store.GetAllGamesAsync(cancellationToken))
            .OrderBy(x => x.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var duplicates = duplicateDetector.FindCrossPlatformDuplicates(allGames);

        return new LibraryResponse(
            allGames.Count,
            duplicates.Count,
            allGames,
            duplicates);
    }
}
