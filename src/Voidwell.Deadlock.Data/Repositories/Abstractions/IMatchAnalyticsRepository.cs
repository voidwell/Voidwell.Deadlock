using Voidwell.Deadlock.Data.Repositories.Models;

namespace Voidwell.Deadlock.Data.Repositories.Abstractions;

/// <summary>
/// Reads the match-level rollups: match length, time of day, objectives and leavers. Every method sums the daily rows
/// that match the filter.
/// </summary>
public interface IMatchAnalyticsRepository
{
    /// <summary>
    /// Matches by length in whole minutes.
    /// </summary>
    Task<List<DurationTotals>> GetDurationTotalsAsync(AnalyticsFilter filter);

    /// <summary>
    /// Matches by the UTC hour they started in.
    /// </summary>
    Task<List<HourTotals>> GetHourTotalsAsync(AnalyticsFilter filter);

    Task<List<ObjectiveTotals>> GetObjectiveTotalsAsync(AnalyticsFilter filter);

    Task<List<LeaverTotals>> GetLeaverTotalsAsync(AnalyticsFilter filter);
}
