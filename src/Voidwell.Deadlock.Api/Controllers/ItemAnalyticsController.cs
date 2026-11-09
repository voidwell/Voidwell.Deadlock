using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Voidwell.Deadlock.Analytics;

namespace Voidwell.Deadlock.Api.Controllers;

/// <summary>
/// Shop item analytics across all heroes. Takes the same period, mode and rank query parameters as the hero analytics.
/// </summary>
[Route("deadlock/analytics/items")]
[OutputCache(PolicyName = CachePolicies.Analytics)]
public class ItemAnalyticsController : Controller
{
    private readonly IAnalyticsQueryResolver _resolver;
    private readonly IItemAnalyticsService _service;

    public ItemAnalyticsController(IAnalyticsQueryResolver resolver, IItemAnalyticsService service)
    {
        _resolver = resolver;
        _service = service;
    }

    /// <summary>
    /// How often each shop item is bought and how its buyers do: win rate, how long they keep it, and when they buy it.
    /// </summary>
    /// <param name="request">The period, mode and rank filters.</param>
    /// <param name="minGames">Leaves out items bought fewer times than this.</param>
    /// <param name="slot">Only weapon, spirit or vitality items.</param>
    /// <param name="itemTier">Only items of this shop tier (1 to 5). Not the same as the rank tiers.</param>
    [HttpGet]
    public async Task<ActionResult> GetItemStatsAsync([FromQuery] AnalyticsRequest request, [FromQuery] long minGames = 100,
        [FromQuery] string? slot = null, [FromQuery] int? itemTier = null)
    {
        var query = await _resolver.ResolveAsync(request);
        return Ok(await _service.GetItemStatsAsync(query, minGames, slot, itemTier));
    }
}
