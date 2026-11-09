using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Voidwell.Deadlock.Analytics;

namespace Voidwell.Deadlock.Api.Controllers;

/// <summary>
/// Team composition analytics. Takes the same period, mode and rank query parameters as the hero analytics.
/// </summary>
[Route("deadlock/analytics/compositions")]
[OutputCache(PolicyName = CachePolicies.Analytics)]
public class CompositionAnalyticsController : Controller
{
    private readonly IAnalyticsQueryResolver _resolver;
    private readonly IMatchupAnalyticsService _service;

    public CompositionAnalyticsController(IAnalyticsQueryResolver resolver, IMatchupAnalyticsService service)
    {
        _resolver = resolver;
        _service = service;
    }

    /// <summary>
    /// How teams do by how many heroes of each archetype (brawler, assassin, marksman, mystic, support) they have. Heroes
    /// with no archetype in the game data, which are the support heroes, count as supports.
    /// </summary>
    /// <param name="request">The period, mode and rank filters.</param>
    /// <param name="minGames">Leaves out compositions with fewer teams than this.</param>
    [HttpGet]
    public async Task<ActionResult> GetCompositionsAsync([FromQuery] AnalyticsRequest request, [FromQuery] long minGames = 100)
    {
        var query = await _resolver.ResolveAsync(request);
        return Ok(await _service.GetCompositionsAsync(query, minGames));
    }
}
