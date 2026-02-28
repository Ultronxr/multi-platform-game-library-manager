using GameLibrary.Api.Models;

namespace GameLibrary.Api.Services;

public sealed class SyncResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public IReadOnlyCollection<OwnedGame> Games { get; init; } = [];

    public static SyncResult Success(IReadOnlyCollection<OwnedGame> games) =>
        new() { IsSuccess = true, Games = games };

    public static SyncResult Failure(string message) =>
        new() { IsSuccess = false, ErrorMessage = message };
}
