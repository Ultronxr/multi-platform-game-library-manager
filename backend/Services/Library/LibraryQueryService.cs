using GameLibrary.Api.Data;
using GameLibrary.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameLibrary.Api.Services;

/// <summary>
/// 库存查询服务实现。
/// </summary>
public sealed class LibraryQueryService(
    IDbContextFactory<GameLibraryDbContext> dbContextFactory,
    IGameLibraryStore store,
    DuplicateDetector duplicateDetector) : ILibraryQueryService
{
    /// <inheritdoc />
    public async Task<IReadOnlyCollection<SavedAccount>> GetAccountsAsync(CancellationToken cancellationToken) =>
        await store.GetAllAccountsAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<LibraryResponse> GetLibraryAsync(bool includeGames, CancellationToken cancellationToken)
    {
        var allGames = (await store.GetAllGamesAsync(cancellationToken))
            .OrderBy(x => x.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var duplicates = duplicateDetector.FindCrossPlatformDuplicates(allGames);
        var gamesForResponse = includeGames ? allGames : [];

        return new LibraryResponse(
            allGames.Count,
            duplicates.Count,
            gamesForResponse,
            duplicates);
    }

    /// <inheritdoc />
    public async Task<LibraryGamesPageResponse> GetLibraryGamesPageAsync(
        LibraryGamesQueryRequest request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize switch
        {
            <= 0 => 20,
            > 100 => 100,
            _ => request.PageSize
        };

        var safeGameTitle = request.GameTitle?.Trim();
        var safeAccountName = request.AccountName?.Trim();
        var safeAccountExternalId = request.AccountExternalId?.Trim();

        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var query = db.OwnedGames
            .AsNoTracking()
            .Join(
                db.PlatformAccounts.AsNoTracking(),
                ownedGame => ownedGame.AccountId,
                account => account.Id,
                (ownedGame, account) => new { OwnedGame = ownedGame, Account = account });

        if (!string.IsNullOrWhiteSpace(safeGameTitle))
        {
            query = query.Where(x => x.OwnedGame.Title.Contains(safeGameTitle));
        }

        if (request.Platform is not null)
        {
            query = query.Where(x => x.OwnedGame.Platform == request.Platform.Value);
        }

        if (!string.IsNullOrWhiteSpace(safeAccountName))
        {
            query = query.Where(x => x.OwnedGame.AccountName.Contains(safeAccountName));
        }

        if (!string.IsNullOrWhiteSpace(safeAccountExternalId))
        {
            query = query.Where(x =>
                (x.Account.ExternalAccountId ?? string.Empty).Contains(safeAccountExternalId));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderBy(x => x.OwnedGame.Title)
            .ThenBy(x => x.OwnedGame.AccountName)
            .ThenBy(x => x.OwnedGame.ExternalGameId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.OwnedGame.ExternalGameId,
                x.OwnedGame.Title,
                x.OwnedGame.Platform,
                x.OwnedGame.AccountName,
                x.Account.ExternalAccountId,
                x.OwnedGame.SyncedAtUtc
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new LibraryGameListItem(
                row.ExternalGameId,
                row.Title,
                row.Platform,
                row.AccountName,
                row.ExternalAccountId,
                Utc8DateTimeFormatter.NormalizeToUtc8(row.SyncedAtUtc)))
            .ToList();

        return new LibraryGamesPageResponse(pageNumber, pageSize, totalCount, items);
    }
}
