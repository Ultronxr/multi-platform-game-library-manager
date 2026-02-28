namespace GameLibrary.Api.Models;

public sealed record DuplicateGroup(
    string NormalizedTitle,
    IReadOnlyCollection<OwnedGame> Games);

public sealed record LibraryResponse(
    int TotalGames,
    int DuplicateGroups,
    IReadOnlyCollection<OwnedGame> Games,
    IReadOnlyCollection<DuplicateGroup> Duplicates);
