namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// How one hero's players levelled one ability on one UTC day. An ability takes four points: the unlock and three upgrades.
/// </summary>
public class HeroAbilityDay : IRollupRow
{
    public int HeroId { get; set; }
    public DateOnly Day { get; set; }
    public short Mode { get; set; }
    public short Tier { get; set; }
    public long AbilityId { get; set; }

    /// <summary>
    /// Games where the player put at least one point into the ability.
    /// </summary>
    public int Games { get; set; }
    public int Wins { get; set; }

    /// <summary>
    /// Games where all four points went into the ability.
    /// </summary>
    public int MaxedGames { get; set; }
    public int MaxedWins { get; set; }

    /// <summary>
    /// Games where this was the first of the player's abilities to get its fourth point.
    /// </summary>
    public int MaxFirstGames { get; set; }
    public int MaxFirstWins { get; set; }

    /// <summary>
    /// Sum of the time of each game's first point, in seconds.
    /// </summary>
    public long UnlockTimeSumS { get; set; }
}
