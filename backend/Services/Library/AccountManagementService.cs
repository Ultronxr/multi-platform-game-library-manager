using GameLibrary.Api.Data;
using GameLibrary.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameLibrary.Api.Services;

/// <summary>
/// 平台账号管理服务实现。
/// </summary>
public sealed class AccountManagementService(
    IDbContextFactory<GameLibraryDbContext> dbContextFactory,
    ISyncService syncService,
    ILogger<AccountManagementService> logger) : IAccountManagementService
{
    /// <inheritdoc />
    public async Task<ServiceOperationResult<int>> ResyncSavedAccountAsync(long accountId, CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var account = await db.PlatformAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == accountId, cancellationToken);
        if (account is null)
        {
            return ServiceOperationResult<int>.Failure(StatusCodes.Status404NotFound, "Account not found.");
        }

        logger.LogInformation(
            "Starting resync for saved account {AccountId} ({Platform}/{AccountName})",
            account.Id,
            account.Platform,
            account.AccountName);

        return account.Platform switch
        {
            GamePlatform.Steam => await ResyncSteamAccountAsync(account, cancellationToken),
            GamePlatform.Epic => await ResyncEpicAccountAsync(account, cancellationToken),
            _ => ServiceOperationResult<int>.Failure(
                StatusCodes.Status400BadRequest,
                $"Unsupported platform: {account.Platform}")
        };
    }

    /// <inheritdoc />
    public async Task<ServiceOperationResult> UpdateSavedAccountAsync(
        long accountId,
        UpdateSavedAccountRequest request,
        CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var account = await db.PlatformAccounts
            .FirstOrDefaultAsync(x => x.Id == accountId, cancellationToken);
        if (account is null)
        {
            return ServiceOperationResult.Failure(StatusCodes.Status404NotFound, "Account not found.");
        }

        var accountName = string.IsNullOrWhiteSpace(request.AccountName)
            ? account.AccountName
            : request.AccountName.Trim();
        if (string.IsNullOrWhiteSpace(accountName))
        {
            return ServiceOperationResult.Failure(StatusCodes.Status400BadRequest, "AccountName is required.");
        }

        var credentialValue = request.CredentialValue is null
            ? account.CredentialValue
            : request.CredentialValue.Trim();
        if (string.IsNullOrWhiteSpace(credentialValue))
        {
            return ServiceOperationResult.Failure(StatusCodes.Status400BadRequest, "CredentialValue cannot be empty.");
        }

        var externalAccountId = request.ExternalAccountId is null
            ? account.ExternalAccountId
            : NormalizeOptional(request.ExternalAccountId);

        if (account.Platform == GamePlatform.Steam && string.IsNullOrWhiteSpace(externalAccountId))
        {
            return ServiceOperationResult.Failure(
                StatusCodes.Status400BadRequest,
                "Steam account requires ExternalAccountId (SteamId).");
        }

        var accountNameChanged = !string.Equals(account.AccountName, accountName, StringComparison.Ordinal);
        account.AccountName = accountName;
        account.ExternalAccountId = externalAccountId;
        account.CredentialValue = credentialValue;
        account.UpdatedAtUtc = Utc8DateTimeFormatter.NowUtc8();

        if (accountNameChanged)
        {
            // 账号名是库存明细中的快照字段，编辑账号名后同步刷新历史展示值。
            var ownedGames = await db.OwnedGames
                .Where(x => x.AccountId == account.Id)
                .ToListAsync(cancellationToken);
            foreach (var ownedGame in ownedGames)
            {
                ownedGame.AccountName = accountName;
            }
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return ServiceOperationResult.Failure(
                StatusCodes.Status409Conflict,
                "Account name already exists for this platform.");
        }

        return ServiceOperationResult.Success(message: "Account updated.");
    }

    /// <inheritdoc />
    public async Task<ServiceOperationResult> DeleteSavedAccountAsync(long accountId, CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var account = await db.PlatformAccounts
            .FirstOrDefaultAsync(x => x.Id == accountId, cancellationToken);
        if (account is null)
        {
            return ServiceOperationResult.Failure(StatusCodes.Status404NotFound, "Account not found.");
        }

        db.PlatformAccounts.Remove(account);
        await db.SaveChangesAsync(cancellationToken);

        return ServiceOperationResult.Success(message: "Account deleted.");
    }

    private async Task<ServiceOperationResult<int>> ResyncSteamAccountAsync(
        Data.Entities.PlatformAccountEntity account,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(account.ExternalAccountId))
        {
            return ServiceOperationResult<int>.Failure(
                StatusCodes.Status400BadRequest,
                "Steam account is missing ExternalAccountId (SteamId).");
        }

        if (string.IsNullOrWhiteSpace(account.CredentialValue))
        {
            return ServiceOperationResult<int>.Failure(
                StatusCodes.Status400BadRequest,
                "Steam account is missing saved API key.");
        }

        return await syncService.SyncSteamAsync(
            new SteamSyncRequest(
                account.ExternalAccountId.Trim(),
                account.CredentialValue.Trim(),
                account.AccountName),
            cancellationToken);
    }

    private async Task<ServiceOperationResult<int>> ResyncEpicAccountAsync(
        Data.Entities.PlatformAccountEntity account,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(account.CredentialValue))
        {
            return ServiceOperationResult<int>.Failure(
                StatusCodes.Status400BadRequest,
                "Epic account is missing saved access token.");
        }

        return await syncService.SyncEpicAsync(
            new EpicSyncRequest(account.CredentialValue.Trim(), account.AccountName),
            cancellationToken);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
