using GameLibrary.Api.Models;

namespace GameLibrary.Api.Services;

public interface IGameLibraryStore
{
    Task SaveAccountAndGamesAsync(
        GamePlatform platform,
        string accountName,
        string? externalAccountId,
        string credentialType,
        string credentialValue,
        IReadOnlyCollection<OwnedGame> games,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<OwnedGame>> GetAllGamesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<SavedAccount>> GetAllAccountsAsync(CancellationToken cancellationToken);
}
