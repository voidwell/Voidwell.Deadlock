using Voidwell.Deadlock.Data.Repositories.Models;

namespace Voidwell.Deadlock.Data.Repositories.Abstractions;

/// <summary>
/// Reads the matchup, synergy, lane and team composition rollups. Every method sums the daily rows that match the filter.
/// </summary>
public interface IMatchupAnalyticsRepository
{
    /// <summary>
    /// One hero's results against each enemy hero.
    /// </summary>
    Task<List<PairTotals>> GetMatchupTotalsAsync(int heroId, AnalyticsFilter filter);

    /// <summary>
    /// One hero's results with each teammate hero.
    /// </summary>
    Task<List<PairTotals>> GetSynergyTotalsAsync(int heroId, AnalyticsFilter filter);

    /// <summary>
    /// One hero's results against each hero it faced in its lane, including the souls lead at nine minutes.
    /// </summary>
    Task<List<PairTotals>> GetLaneTotalsAsync(int heroId, AnalyticsFilter filter);

    Task<List<CompositionTotals>> GetCompositionTotalsAsync(AnalyticsFilter filter);
}
