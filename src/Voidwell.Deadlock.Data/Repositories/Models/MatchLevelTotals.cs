namespace Voidwell.Deadlock.Data.Repositories.Models;

public class DurationTotals
{
    public int Minute { get; set; }
    public long Matches { get; set; }
}

public class HourTotals
{
    public int Hour { get; set; }
    public long Matches { get; set; }
}

/// <summary>
/// How teams fared with and without one objective, summed over a filter. See <c>MatchObjectiveDay</c>.
/// </summary>
public class ObjectiveTotals
{
    public int Objective { get; set; }
    public long LostGames { get; set; }
    public long LostWins { get; set; }
    public long HeldGames { get; set; }
    public long HeldWins { get; set; }
}

/// <summary>
/// How teams fared with a given number of leavers on each side, summed over a filter.
/// </summary>
public class LeaverTotals
{
    public int OwnLeavers { get; set; }
    public int EnemyLeavers { get; set; }
    public long Games { get; set; }
    public long Wins { get; set; }
    public long Players { get; set; }
    public long LeaverPlayers { get; set; }
}
