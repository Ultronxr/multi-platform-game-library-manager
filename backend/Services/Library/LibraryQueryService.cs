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
        var groupedGames = CollapseSameGamePerAccount(allGames);

        var duplicates = duplicateDetector.FindCrossPlatformDuplicates(groupedGames);
        var gamesForResponse = includeGames ? groupedGames : [];

        return new LibraryResponse(
            groupedGames.Count,
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

        var groupedQuery = query
            .GroupBy(x => new
            {
                x.OwnedGame.AccountId,
                x.OwnedGame.Platform,
                x.OwnedGame.AccountName,
                x.Account.ExternalAccountId,
                x.OwnedGame.Title
            });

        var totalCount = await groupedQuery.CountAsync(cancellationToken);
        if (totalCount == 0)
        {
            return new LibraryGamesPageResponse(pageNumber, pageSize, totalCount, []);
        }

        var pageGroups = await groupedQuery
            .Select(g => new
            {
                g.Key.AccountId,
                g.Key.Platform,
                g.Key.AccountName,
                g.Key.ExternalAccountId,
                g.Key.Title,
                SyncedAtUtc = g.Max(x => x.OwnedGame.SyncedAtUtc),
                GroupItemCount = g.Count()
            })
            .OrderBy(x => x.Title)
            .ThenBy(x => x.AccountName)
            .ThenBy(x => x.Platform)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var accountIds = pageGroups.Select(x => x.AccountId).Distinct().ToList();
        var titles = pageGroups.Select(x => x.Title).Distinct().ToList();
        var pageGroupKeys = pageGroups
            .Select(x => BuildGroupKey(x.AccountId, x.Platform, x.Title))
            .ToHashSet(StringComparer.Ordinal);

        var childRows = await query
            .Where(x =>
                accountIds.Contains(x.OwnedGame.AccountId) &&
                titles.Contains(x.OwnedGame.Title))
            .Select(x => new
            {
                x.OwnedGame.AccountId,
                x.OwnedGame.Platform,
                x.OwnedGame.AccountName,
                x.Account.ExternalAccountId,
                x.OwnedGame.Title,
                x.OwnedGame.ExternalGameId,
                x.OwnedGame.EpicAppName,
                x.OwnedGame.SyncedAtUtc
            })
            .ToListAsync(cancellationToken);

        var groupedChildren = childRows
            .Where(row => pageGroupKeys.Contains(BuildGroupKey(row.AccountId, row.Platform, row.Title)))
            .GroupBy(row => BuildGroupKey(row.AccountId, row.Platform, row.Title), StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(row => row.ExternalGameId, StringComparer.OrdinalIgnoreCase)
                    .Select(row => new LibraryGameGroupItem(
                        row.ExternalGameId,
                        row.EpicAppName,
                        Utc8DateTimeFormatter.NormalizeToUtc8(row.SyncedAtUtc)))
                    .ToList(),
                StringComparer.Ordinal);

        var items = pageGroups
            .Select(group =>
            {
                var groupKey = BuildGroupKey(group.AccountId, group.Platform, group.Title);
                var groupItems = groupedChildren.TryGetValue(groupKey, out var details)
                    ? details
                    : [];
                var representativeAppName = groupItems
                    .Select(x => x.EpicAppName)
                    .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

                return new LibraryGameListItem(
                    groupKey,
                    group.Title,
                    group.Platform,
                    group.AccountName,
                    group.ExternalAccountId,
                    Utc8DateTimeFormatter.NormalizeToUtc8(group.SyncedAtUtc),
                    group.GroupItemCount,
                    representativeAppName,
                    groupItems);
            })
            .ToList();

        return new LibraryGamesPageResponse(pageNumber, pageSize, totalCount, items);
    }

    private static string BuildGroupKey(long accountId, GamePlatform platform, string title) =>
        $"{accountId}|{platform}|{title}";

    private static IReadOnlyCollection<OwnedGame> CollapseSameGamePerAccount(IReadOnlyCollection<OwnedGame> games)
    {
        // 统计口径按“同平台 + 同账号 + 同游戏名”聚合，避免 Epic 同游戏附加项重复计数。
        return games
            .Where(x => !string.IsNullOrWhiteSpace(x.Title))
            .GroupBy(
                x => new
                {
                    x.Platform,
                    AccountName = x.AccountName.Trim().ToUpperInvariant(),
                    Title = x.Title.Trim().ToUpperInvariant()
                })
            .Select(group =>
            {
                var representative = group
                    .OrderByDescending(x => x.SyncedAtUtc)
                    .ThenBy(x => x.ExternalId, StringComparer.OrdinalIgnoreCase)
                    .First();
                var latestSyncedAt = group.Max(x => x.SyncedAtUtc);
                return new OwnedGame(
                    representative.ExternalId,
                    representative.Title,
                    representative.Platform,
                    representative.AccountName,
                    latestSyncedAt,
                    representative.EpicAppName);
            })
            .OrderBy(x => x.Title, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.AccountName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Platform)
            .ToList();
    }
}
