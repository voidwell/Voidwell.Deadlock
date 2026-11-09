using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Voidwell.Deadlock.Analytics;

namespace Voidwell.Deadlock.Api.Controllers;

/// <summary>
/// How matches develop over time. Takes the same period, mode and rank query parameters as the hero analytics. Stats are
/// sampled at 3, 6, 9, 12 and 15 minutes, then every 15 minutes (30, 45, 60 and 75), so the later part of a curve has fewer points.
/// </summary>
[Route("deadlock/analytics")]
[OutputCache(PolicyName = CachePolicies.Analytics)]
public class CurveAnalyticsController : Controller
{
    private readonly IAnalyticsQueryResolver _resolver;
    private readonly ICurveAnalyticsService _service;

    public CurveAnalyticsController(IAnalyticsQueryResolver resolver, ICurveAnalyticsService service)
    {
        _resolver = resolver;
        _service = service;
    }

    /// <summary>
    /// A hero's average souls, level, kills, damage, healing and power at each sample time, for all its players and for
    /// those who went on to win and to lose. Later times only cover matches that lasted that long.
    /// </summary>
    /// <param name="heroId">The hero.</param>
    /// <param name="request">The period, mode and rank filters.</param>
    /// <param name="minSamples">Leaves out averages taken from fewer players than this.</param>
    [HttpGet("heroes/{heroId:int}/curves")]
    public async Task<ActionResult> GetHeroCurvesAsync(int heroId, [FromQuery] AnalyticsRequest request, [FromQuery] long minSamples = 50)
    {
        var query = await _resolver.ResolveAsync(request);

        var curves = await _service.GetHeroCurvesAsync(heroId, query, minSamples);
        return curves == null ? NotFound() : Ok(curves);
    }

    /// <summary>
    /// Where a hero's souls come from over the match: kills, lane creeps, neutral camps, bosses, treasure, and other.
    /// </summary>
    [HttpGet("heroes/{heroId:int}/souls-sources")]
    public async Task<ActionResult> GetHeroSoulsSourcesAsync(int heroId, [FromQuery] AnalyticsRequest request, [FromQuery] long minSamples = 50)
    {
        var query = await _resolver.ResolveAsync(request);

        var sources = await _service.GetSoulsSourcesAsync(heroId, query, minSamples);
        return sources == null ? NotFound() : Ok(sources);
    }

    /// <summary>
    /// How often the team ahead on souls goes on to win, at each sample time and for each size of lead. One minus that is
    /// the comeback rate.
    /// </summary>
    /// <param name="request">The period, mode and rank filters.</param>
    /// <param name="minute">Only this sample time: 3, 6, 9, 12, 15, 30, 45, 60 or 75.</param>
    /// <param name="minGames">Leaves out lead sizes seen in fewer matches than this.</param>
    [HttpGet("lead")]
    public async Task<ActionResult> GetLeadAsync([FromQuery] AnalyticsRequest request, [FromQuery] int? minute = null, [FromQuery] long minGames = 50)
    {
        var query = await _resolver.ResolveAsync(request);
        return Ok(await _service.GetLeadAsync(query, minute, minGames));
    }
}
