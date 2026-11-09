namespace Voidwell.Deadlock.Analytics;

/// <summary>
/// Sorts heroes into S to D tiers by win rate. Every match has one winning team, so the average win rate is 50% and
/// the tiers are spread around it.
/// </summary>
public static class TierRating
{
    public const string Method = "By win rate: S at 53% and above, A from 51%, B from 49%, C from 47%, D below. " +
        "Heroes with fewer games than minGames are left out.";

    public static string FromWinRate(double winRate)
    {
        return winRate switch
        {
            >= 0.53 => "S",
            >= 0.51 => "A",
            >= 0.49 => "B",
            >= 0.47 => "C",
            _ => "D"
        };
    }
}
