namespace Voidwell.Deadlock.Data.Repositories.Abstractions;

public interface IRollupRepository
{
    /// <summary>
    /// The newest UTC day that has been rolled up, or null if nothing has.
    /// </summary>
    Task<DateOnly?> GetLatestRollupDayAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// The oldest and newest UTC days that have stored matches, or null if there are none.
    /// </summary>
    Task<(DateOnly First, DateOnly Last)?> GetMatchDayRangeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Recomputes every rollup for one UTC day from the match tables, replacing what was stored for that day.
    /// </summary>
    Task RebuildDayAsync(DateOnly day, CancellationToken cancellationToken = default);
}
