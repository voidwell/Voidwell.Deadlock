namespace Voidwell.Deadlock.Data.Repositories.Models;

/// <summary>
/// A hero's rollup rows summed over a filter. <see cref="Day"/> or <see cref="Tier"/> is set when the sum was
/// grouped by it.
/// </summary>
public class HeroTotals
{
    public int HeroId { get; set; }
    public DateOnly? Day { get; set; }
    public int? Tier { get; set; }

    public long Games { get; set; }
    public long Wins { get; set; }
    public long Kills { get; set; }
    public long Deaths { get; set; }
    public long Assists { get; set; }
    public long NetWorth { get; set; }
    public long LastHits { get; set; }
    public long Denies { get; set; }
    public long DurationSumS { get; set; }
    public long StatGames { get; set; }
    public long PlayerDamage { get; set; }
    public long PlayerDamageTaken { get; set; }
    public long BossDamage { get; set; }
    public long PlayerHealing { get; set; }
    public long ShotsHit { get; set; }
    public long ShotsMissed { get; set; }
}
