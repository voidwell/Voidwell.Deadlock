namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// One hero's results on one UTC day, split by match mode and badge tier. Rollup of <see cref="MatchPlayer"/>, with
/// damage, healing and shot totals taken from each player's final stat sample.
/// </summary>
public class HeroDay : IRollupRow
{
    public DateOnly Day { get; set; }
    public short Mode { get; set; }
    public short Tier { get; set; }
    public int HeroId { get; set; }

    public int Games { get; set; }
    public int Wins { get; set; }
    public long Kills { get; set; }
    public long Deaths { get; set; }
    public long Assists { get; set; }
    public long NetWorth { get; set; }
    public long LastHits { get; set; }
    public long Denies { get; set; }

    /// <summary>
    /// Sum of the lengths of the matches played, for per-minute rates.
    /// </summary>
    public long DurationSumS { get; set; }

    /// <summary>
    /// Games with a final stat sample. The totals below only cover these, so their averages divide by this.
    /// </summary>
    public int StatGames { get; set; }

    public long PlayerDamage { get; set; }
    public long PlayerDamageTaken { get; set; }
    public long BossDamage { get; set; }
    public long PlayerHealing { get; set; }
    public long ShotsHit { get; set; }
    public long ShotsMissed { get; set; }
}
