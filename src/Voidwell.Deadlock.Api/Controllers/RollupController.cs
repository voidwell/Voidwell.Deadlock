using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voidwell.Deadlock.Analytics;
using Voidwell.Deadlock.Api.Authentication;
using Voidwell.Deadlock.Data.Repositories.Abstractions;

namespace Voidwell.Deadlock.Api.Controllers;

[Route("deadlock/rollups")]
[Authorize(Roles = AuthConstants.Roles.Administrator)]
public class RollupController : Controller
{
    private readonly RollupUpdater _rollupUpdater;
    private readonly IRollupRepository _rollupRepository;

    public RollupController(RollupUpdater rollupUpdater, IRollupRepository rollupRepository)
    {
        _rollupUpdater = rollupUpdater;
        _rollupRepository = rollupRepository;
    }

    /// <summary>
    /// Recomputes the rollups for a range of days in the background; progress shows in the RollupUpdater service status.
    /// Defaults to every day that has matches. Run it once the match history has finished loading.
    /// </summary>
    [HttpPost("rebuild")]
    public async Task<ActionResult> PostRebuildAsync(DateOnly? from, DateOnly? to)
    {
        var matchDays = await _rollupRepository.GetMatchDayRangeAsync();
        if (matchDays == null)
        {
            return BadRequest("There are no matches to roll up.");
        }

        var start = from ?? matchDays.Value.First;
        var end = to ?? matchDays.Value.Last;

        if (start > end)
        {
            return BadRequest("from must not be after to.");
        }

        if (!_rollupUpdater.TryStartRebuild(start, end))
        {
            return Conflict("A rollup job is already running.");
        }

        return Accepted(new { From = start, To = end });
    }
}
