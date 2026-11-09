using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Voidwell.Deadlock.Analytics;

namespace Voidwell.Deadlock.Api.Controllers;

/// <summary>
/// Analytics about matches as a whole rather than about heroes. Takes the same period, mode and rank query parameters as the
/// hero analytics.
/// </summary>
[Route("deadlock/analytics")]
[OutputCache(PolicyName = CachePolicies.Analytics)]
public class MatchAnalyticsController : Controller
{
    private readonly IAnalyticsQueryResolver _resolver;
    private readonly IMatchAnalyticsService _service;

    public MatchAnalyticsController(IAnalyticsQueryResolver resolver, IMatchAnalyticsService service)
    {
        _resolver = resolver;
        _service = service;
    }

    /// <summary>
    /// How many matches were played each day and in each mode, how long they last (average, percentiles and a histogram),
    /// which hours of the day (UTC) they start in, and how often each team wins.
    /// </summary>
    [HttpGet("overview")]
    public async Task<ActionResult> GetOverviewAsync([FromQuery] AnalyticsRequest request)
    {
        var query = await _resolver.ResolveAsync(request);
        return Ok(await _service.GetOverviewAsync(query));
    }

    /// <summary>
    /// How matches are spread across rank tiers. With <c>mode=all</c>, unranked matches are shown as their own group because
    /// they have no rank.
    /// </summary>
    [HttpGet("ranks")]
    public async Task<ActionResult> GetRanksAsync([FromQuery] AnalyticsRequest request)
    {
        var query = await _resolver.ResolveAsync(request);
        return Ok(await _service.GetRanksAsync(query));
    }

    /// <summary>
    /// How teams do when each objective is destroyed or still standing at the end of a match. The game records the state at
    /// the end, not when objectives fell. Objectives that were never destroyed are left out.
    /// </summary>
    /// <param name="request">The period, mode and rank filters.</param>
    /// <param name="minGames">Leaves out objectives destroyed in fewer team-matches than this.</param>
    [HttpGet("objectives")]
    public async Task<ActionResult> GetObjectivesAsync([FromQuery] AnalyticsRequest request, [FromQuery] long minGames = 1)
    {
        var query = await _resolver.ResolveAsync(request);
        return Ok(await _service.GetObjectivesAsync(query, minGames));
    }

    /// <summary>
    /// How often players leave a match, and how a team does with leavers on its side or on the enemy's.
    /// </summary>
    [HttpGet("leavers")]
    public async Task<ActionResult> GetLeaversAsync([FromQuery] AnalyticsRequest request)
    {
        var query = await _resolver.ResolveAsync(request);
        return Ok(await _service.GetLeaversAsync(query));
    }
}
