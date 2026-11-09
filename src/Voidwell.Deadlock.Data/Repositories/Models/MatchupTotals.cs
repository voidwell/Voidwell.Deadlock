namespace Voidwell.Deadlock.Data.Repositories.Models;

/// <summary>
/// A hero's results against, or alongside, one other hero, summed over a filter.
/// </summary>
public class PairTotals
{
    /// <summary>
    /// The other hero: the enemy for counters and lane matchups, the teammate for synergies.
    /// </summary>
    public int OtherHeroId { get; set; }
    public long Games { get; set; }
    public long Wins { get; set; }

    // Lane matchups only.
    public long LeadGames { get; set; }
    public long LeadSum { get; set; }
}

/// <summary>
/// Teams with a given number of heroes of each archetype, summed over a filter.
/// </summary>
public class CompositionTotals
{
    public int Brawlers { get; set; }
    public int Assassins { get; set; }
    public int Marksmen { get; set; }
    public int Mystics { get; set; }
    public int Supports { get; set; }
    public long Games { get; set; }
    public long Wins { get; set; }
}
