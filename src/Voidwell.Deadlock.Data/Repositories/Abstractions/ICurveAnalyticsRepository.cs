using Voidwell.Deadlock.Data.Repositories.Models;

namespace Voidwell.Deadlock.Data.Repositories.Abstractions;

/// <summary>
/// Reads the curve and lead rollups. Every method sums the daily rows that match the filter.
/// </summary>
public interface ICurveAnalyticsRepository
{
    /// <summary>
    /// One hero's stats at each sample time, one row for the players who won and one for those who lost.
    /// </summary>
    Task<List<CurveTotals>> GetHeroCurveTotalsAsync(int heroId, AnalyticsFilter filter);

    /// <summary>
    /// Matches by souls lead at each sample time, one row per lead bucket.
    /// </summary>
    Task<List<LeadTotals>> GetLeadTotalsAsync(AnalyticsFilter filter);
}
