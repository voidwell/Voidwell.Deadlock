using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Voidwell.Deadlock.Analytics;

namespace Voidwell.Deadlock.Api.Controllers;

/// <summary>
/// Hero analytics computed from match data. Every endpoint takes the same query parameters: from/to/days or patch for the
/// period, mode (ranked by default, unranked or all) and minTier/maxTier for ranked matches.
/// </summary>
[Route("deadlock/analytics/heroes")]
[OutputCache(PolicyName = CachePolicies.Analytics)]
public class HeroAnalyticsController : Controller
{
    private readonly IAnalyticsQueryResolver _resolver;
    private readonly IHeroAnalyticsService _service;
    private readonly IItemAnalyticsService _itemService;
    private readonly IMatchupAnalyticsService _matchupService;

    public HeroAnalyticsController(IAnalyticsQueryResolver resolver, IHeroAnalyticsService service, IItemAnalyticsService itemService,
        IMatchupAnalyticsService matchupService)
    {
        _resolver = resolver;
        _service = service;
        _itemService = itemService;
        _matchupService = matchupService;
    }

    /// <summary>
    /// Win rate, pick rate, ban rate and per-minute averages for every hero.
    /// </summary>
    /// <param name="request">The period, mode and rank filters.</param>
    /// <param name="minGames">Leaves out heroes with fewer games than this.</param>
    [HttpGet]
    public async Task<ActionResult> GetHeroStatsAsync([FromQuery] AnalyticsRequest request, [FromQuery] long minGames = 1)
    {
        var query = await _resolver.ResolveAsync(request);
        return Ok(await _service.GetHeroStatsAsync(query, minGames));
    }

    /// <summary>
    /// Heroes ranked by win rate and sorted into S to D tiers.
    /// </summary>
    [HttpGet("tierlist")]
    public async Task<ActionResult> GetTierListAsync([FromQuery] AnalyticsRequest request, [FromQuery] long minGames = 100)
    {
        var query = await _resolver.ResolveAsync(request);
        return Ok(await _service.GetTierListAsync(query, minGames));
    }

    /// <summary>
    /// How each hero's win rate, pick rate and ban rate changed against an earlier period. Without compare parameters
    /// the earlier period is the one of equal length just before the requested one.
    /// </summary>
    [HttpGet("trends")]
    public async Task<ActionResult> GetTrendsAsync([FromQuery] AnalyticsRequest request, [FromQuery] long minGames = 100)
    {
        var query = await _resolver.ResolveAsync(request);
        var baseline = await _resolver.ResolveBaselineAsync(request, query);

        return Ok(await _service.GetTrendsAsync(query, baseline, minGames));
    }

    /// <summary>
    /// One hero's results, with its daily trend and its results by rank tier.
    /// </summary>
    [HttpGet("{heroId:int}")]
    public async Task<ActionResult> GetHeroAsync(int heroId, [FromQuery] AnalyticsRequest request)
    {
        var query = await _resolver.ResolveAsync(request);

        var detail = await _service.GetHeroDetailAsync(heroId, query);
        if (detail == null)
        {
            return NotFound();
        }

        return Ok(detail);
    }

    /// <summary>
    /// The shop items one hero's players buy, with win rate, how long they keep each item and when they buy it.
    /// </summary>
    /// <param name="heroId">The hero.</param>
    /// <param name="request">The period, mode and rank filters.</param>
    /// <param name="minGames">Leaves out items bought fewer times than this.</param>
    /// <param name="slot">Only weapon, spirit or vitality items.</param>
    /// <param name="itemTier">Only items of this shop tier (1 to 5). Not the same as the rank tiers.</param>
    [HttpGet("{heroId:int}/items")]
    public async Task<ActionResult> GetHeroItemsAsync(int heroId, [FromQuery] AnalyticsRequest request, [FromQuery] long minGames = 30,
        [FromQuery] string? slot = null, [FromQuery] int? itemTier = null)
    {
        var query = await _resolver.ResolveAsync(request);

        var items = await _itemService.GetHeroItemsAsync(heroId, query, minGames, slot, itemTier);
        return items == null ? NotFound() : Ok(items);
    }

    /// <summary>
    /// Results by how a hero's tier 3 and higher items split across weapon, spirit and vitality. Exact item sets are too
    /// varied to compare, but this split shows which kind of build works.
    /// </summary>
    [HttpGet("{heroId:int}/builds")]
    public async Task<ActionResult> GetHeroBuildsAsync(int heroId, [FromQuery] AnalyticsRequest request, [FromQuery] long minGames = 30)
    {
        var query = await _resolver.ResolveAsync(request);

        var shapes = await _itemService.GetHeroBuildShapesAsync(heroId, query, minGames);
        return shapes == null ? NotFound() : Ok(shapes);
    }

    /// <summary>
    /// How often a hero's players use each community build, and how they do with it.
    /// </summary>
    /// <param name="heroId">The hero.</param>
    /// <param name="request">The period, mode and rank filters.</param>
    /// <param name="minGames">Leaves out builds with fewer games than this.</param>
    /// <param name="limit">How many builds to return, most played first.</param>
    [HttpGet("{heroId:int}/community-builds")]
    public async Task<ActionResult> GetHeroCommunityBuildsAsync(int heroId, [FromQuery] AnalyticsRequest request, [FromQuery] long minGames = 50,
        [FromQuery] int limit = 25)
    {
        var query = await _resolver.ResolveAsync(request);

        var builds = await _itemService.GetHeroCommunityBuildsAsync(heroId, query, minGames, limit);
        return builds == null ? NotFound() : Ok(builds);
    }

    /// <summary>
    /// Which ability players max first, when they unlock each one, and what they pick at each ability point.
    /// </summary>
    [HttpGet("{heroId:int}/ability-order")]
    public async Task<ActionResult> GetHeroAbilityOrderAsync(int heroId, [FromQuery] AnalyticsRequest request)
    {
        var query = await _resolver.ResolveAsync(request);

        var order = await _itemService.GetHeroAbilityOrderAsync(heroId, query);
        return order == null ? NotFound() : Ok(order);
    }

    /// <summary>
    /// How a hero does against each enemy hero, best matchups first. <c>advantage</c> is the win rate minus what the two
    /// heroes' overall win rates alone would predict.
    /// </summary>
    /// <param name="heroId">The hero.</param>
    /// <param name="request">The period, mode and rank filters.</param>
    /// <param name="minGames">Leaves out heroes faced fewer times than this.</param>
    [HttpGet("{heroId:int}/counters")]
    public async Task<ActionResult> GetHeroCountersAsync(int heroId, [FromQuery] AnalyticsRequest request, [FromQuery] long minGames = 100)
    {
        var query = await _resolver.ResolveAsync(request);

        var counters = await _matchupService.GetCountersAsync(heroId, query, minGames);
        return counters == null ? NotFound() : Ok(counters);
    }

    /// <summary>
    /// How a hero does with each teammate hero, best pairings first.
    /// </summary>
    [HttpGet("{heroId:int}/synergies")]
    public async Task<ActionResult> GetHeroSynergiesAsync(int heroId, [FromQuery] AnalyticsRequest request, [FromQuery] long minGames = 100)
    {
        var query = await _resolver.ResolveAsync(request);

        var synergies = await _matchupService.GetSynergiesAsync(heroId, query, minGames);
        return synergies == null ? NotFound() : Ok(synergies);
    }

    /// <summary>
    /// How a hero does against each hero it faces in its lane, with the average souls lead at nine minutes.
    /// </summary>
    [HttpGet("{heroId:int}/lane")]
    public async Task<ActionResult> GetHeroLaneAsync(int heroId, [FromQuery] AnalyticsRequest request, [FromQuery] long minGames = 100)
    {
        var query = await _resolver.ResolveAsync(request);

        var lane = await _matchupService.GetLaneMatchupsAsync(heroId, query, minGames);
        return lane == null ? NotFound() : Ok(lane);
    }
}
