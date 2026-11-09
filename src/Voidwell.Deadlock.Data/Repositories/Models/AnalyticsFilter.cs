namespace Voidwell.Deadlock.Data.Repositories.Models;

/// <summary>
/// Which rollup rows a query covers: an inclusive range of UTC days, and the match modes and badge tiers to include.
/// </summary>
public class AnalyticsFilter
{
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }

    /// <summary>
    /// Match modes to include: 1 for unranked, 4 for ranked.
    /// </summary>
    public required List<short> Modes { get; init; }

    public int MinTier { get; init; }
    public int MaxTier { get; init; }
}
