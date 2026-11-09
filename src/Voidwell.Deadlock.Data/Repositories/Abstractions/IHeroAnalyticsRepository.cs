using Voidwell.Deadlock.Data.Repositories.Models;

namespace Voidwell.Deadlock.Data.Repositories.Abstractions;

/// <summary>
/// Reads the hero rollups. Every method sums the daily rows that match the filter.
/// </summary>
public interface IHeroAnalyticsRepository
{
    /// <summary>
    /// The newest UTC day with rolled-up hero data, or null if there is none.
    /// </summary>
    Task<DateOnly?> GetLatestDayAsync();

    /// <summary>
    /// Matches in the filter, in total.
    /// </summary>
    Task<MatchTotals> GetMatchTotalsAsync(AnalyticsFilter filter);

    /// <summary>
    /// Matches in the filter, one row per day.
    /// </summary>
    Task<List<MatchTotals>> GetMatchTotalsByDayAsync(AnalyticsFilter filter);

    /// <summary>
    /// Matches in the filter, one row per badge tier.
    /// </summary>
    Task<List<MatchTotals>> GetMatchTotalsByTierAsync(AnalyticsFilter filter);

    /// <summary>
    /// Matches in the filter, one row per match mode (1 for unranked, 4 for ranked).
    /// </summary>
    Task<List<MatchTotals>> GetMatchTotalsByModeAsync(AnalyticsFilter filter);

    /// <summary>
    /// Every hero's totals, one row per hero.
    /// </summary>
    Task<List<HeroTotals>> GetHeroTotalsAsync(AnalyticsFilter filter);

    /// <summary>
    /// One hero's totals, one row per day.
    /// </summary>
    Task<List<HeroTotals>> GetHeroTotalsByDayAsync(int heroId, AnalyticsFilter filter);

    /// <summary>
    /// One hero's totals, one row per badge tier.
    /// </summary>
    Task<List<HeroTotals>> GetHeroTotalsByTierAsync(int heroId, AnalyticsFilter filter);

    /// <summary>
    /// Bans per hero in the filter.
    /// </summary>
    Task<List<HeroBanTotals>> GetBanTotalsAsync(AnalyticsFilter filter);
}
