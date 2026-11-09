using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Models;

namespace Voidwell.Deadlock.Analytics;

/// <summary>
/// A validated query: a day range, a mode and a rank tier range.
/// </summary>
public class AnalyticsQuery
{
    public const int HighestTier = 11;

    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public AnalyticsMode Mode { get; init; }

    /// <summary>
    /// Rank tier limits for ranked matches. Null when the query does not limit them.
    /// </summary>
    public int? MinTier { get; init; }
    public int? MaxTier { get; init; }

    public int DayCount => To.DayNumber - From.DayNumber + 1;

    public AnalyticsFilter ToFilter()
    {
        return Mode switch
        {
            AnalyticsMode.Ranked => new AnalyticsFilter
            {
                From = From,
                To = To,
                Modes = [MatchModes.Ranked],
                MinTier = MinTier ?? 0,
                MaxTier = MaxTier ?? HighestTier
            },
            AnalyticsMode.Unranked => new AnalyticsFilter
            {
                From = From,
                To = To,
                Modes = [MatchModes.Unranked],
                MinTier = MatchDay.UnrankedTier,
                MaxTier = MatchDay.UnrankedTier
            },
            _ => new AnalyticsFilter
            {
                From = From,
                To = To,
                Modes = [MatchModes.Unranked, MatchModes.Ranked],
                MinTier = MatchDay.UnrankedTier,
                MaxTier = HighestTier
            }
        };
    }

    private static class MatchModes
    {
        public const short Unranked = (short)Data.Models.MatchMode.Unranked;
        public const short Ranked = (short)Data.Models.MatchMode.Ranked;
    }
}
