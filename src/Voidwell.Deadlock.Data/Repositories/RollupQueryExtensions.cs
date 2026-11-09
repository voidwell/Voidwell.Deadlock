using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Models;

namespace Voidwell.Deadlock.Data.Repositories;

internal static class RollupQueryExtensions
{
    /// <summary>
    /// Keeps the rollup rows inside the filter's days, match modes and badge tiers.
    /// </summary>
    public static IQueryable<T> InFilter<T>(this IQueryable<T> query, AnalyticsFilter filter)
        where T : class, IRollupRow
    {
        var modes = filter.Modes;
        return query.Where(a => a.Day >= filter.From && a.Day <= filter.To && modes.Contains(a.Mode) && a.Tier >= filter.MinTier && a.Tier <= filter.MaxTier);
    }
}
