using GameLibrary.Api.Data;
using GameLibrary.Api.Data.Entities;
using GameLibrary.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameLibrary.Api.Services;

public sealed class EfCoreGameLibraryStore(IDbContextFactory<GameLibraryDbContext> dbContextFactory) : IGameLibraryStore
{
    public async Task SaveAccountAndGamesAsync(
        GamePlatform platform,
        string accountName,
        string? externalAccountId,
        string credentialType,
        string credentialValue,
        IReadOnlyCollection<OwnedGame> games,
        CancellationToken cancellationToken)
    {
        var safeAccountName = string.IsNullOrWhiteSpace(accountName)
            ? throw new ArgumentException("Account name is required.", nameof(accountName))
            : accountName.Trim();
        var safeCredentialType = string.IsNullOrWhiteSpace(credentialType)
            ? throw new ArgumentException("Credential type is required.", nameof(credentialType))
            : credentialType.Trim();
        var safeCredentialValue = string.IsNullOrWhiteSpace(credentialValue)
            ? throw new ArgumentException("Credential value is required.", nameof(credentialValue))
            : credentialValue.Trim();

        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var account = await db.PlatformAccounts
            .FirstOrDefaultAsync(
                x => x.Platform == platform && x.AccountName == safeAccountName,
                cancellationToken);

        if (account is null)
        {
            account = new PlatformAccountEntity
            {
                Platform = platform,
                AccountName = safeAccountName,
                ExternalAccountId = NormalizeOptional(externalAccountId),
                CredentialType = safeCredentialType,
                CredentialValue = safeCredentialValue,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
                LastSyncedAtUtc = now
            };

            db.PlatformAccounts.Add(account);
            await db.SaveChangesAsync(cancellationToken);
        }
        else
        {
            account.ExternalAccountId = NormalizeOptional(externalAccountId);
            account.CredentialType = safeCredentialType;
            account.CredentialValue = safeCredentialValue;
            account.UpdatedAtUtc = now;
            account.LastSyncedAtUtc = now;
            await db.SaveChangesAsync(cancellationToken);
        }

        var existingGames = await db.OwnedGames
            .Where(x => x.AccountId == account.Id)
            .ToListAsync(cancellationToken);
        if (existingGames.Count > 0)
        {
            db.OwnedGames.RemoveRange(existingGames);
            await db.SaveChangesAsync(cancellationToken);
        }

        var normalizedGames = games
            .Where(x => !string.IsNullOrWhiteSpace(x.Title))
            .GroupBy(x => x.ExternalId, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        var entities = normalizedGames.Select(game => new OwnedGameEntity
        {
            AccountId = account.Id,
            Platform = platform,
            AccountName = safeAccountName,
            ExternalGameId = game.ExternalId,
            Title = game.Title,
            NormalizedTitle = TitleNormalizer.Normalize(game.Title),
            SyncedAtUtc = now,
            CreatedAtUtc = now
        });

        db.OwnedGames.AddRange(entities);
        await db.SaveChangesAsync(cancellationToken);

        await tx.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<OwnedGame>> GetAllGamesAsync(CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var games = await db.OwnedGames
            .AsNoTracking()
            .OrderBy(x => x.Title)
            .ToListAsync(cancellationToken);

        return games.Select(game => new OwnedGame(
                game.ExternalGameId,
                game.Title,
                game.Platform,
                game.AccountName,
                EnsureUtc(game.SyncedAtUtc)))
            .ToList();
    }

    public async Task<IReadOnlyCollection<SavedAccount>> GetAllAccountsAsync(CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var accounts = await db.PlatformAccounts
            .AsNoTracking()
            .OrderBy(x => x.Platform)
            .ThenBy(x => x.AccountName)
            .ToListAsync(cancellationToken);

        return accounts.Select(account => new SavedAccount(
                account.Id,
                account.Platform,
                account.AccountName,
                account.ExternalAccountId,
                account.CredentialType,
                MaskCredential(account.CredentialValue),
                EnsureUtc(account.CreatedAtUtc),
                EnsureUtc(account.UpdatedAtUtc),
                account.LastSyncedAtUtc is null ? null : EnsureUtc(account.LastSyncedAtUtc.Value)))
            .ToList();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DateTime EnsureUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static string MaskCredential(string credential)
    {
        if (string.IsNullOrWhiteSpace(credential))
        {
            return string.Empty;
        }

        var value = credential.Trim();
        if (value.Length <= 8)
        {
            return new string('*', value.Length);
        }

        return $"{value[..4]}****{value[^4..]}";
    }
}
