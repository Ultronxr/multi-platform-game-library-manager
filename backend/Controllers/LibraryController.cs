using GameLibrary.Api.Models;
using GameLibrary.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameLibrary.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class LibraryController(
    IGameLibraryStore store,
    DuplicateDetector duplicateDetector) : ControllerBase
{
    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccounts(CancellationToken cancellationToken)
    {
        var accounts = await store.GetAllAccountsAsync(cancellationToken);
        return Ok(accounts);
    }

    [HttpGet("library")]
    public async Task<IActionResult> GetLibrary(CancellationToken cancellationToken)
    {
        var allGames = (await store.GetAllGamesAsync(cancellationToken))
            .OrderBy(x => x.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var duplicates = duplicateDetector.FindCrossPlatformDuplicates(allGames);

        return Ok(new LibraryResponse(
            allGames.Count,
            duplicates.Count,
            allGames,
            duplicates));
    }
}
