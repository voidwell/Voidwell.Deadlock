namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// The summed stats of one hero's players at one sample time of the match, on one UTC day, split by whether they went on to
/// win. Divide a sum by <see cref="Samples"/> for the average. The stats are cumulative, so each value is the total so far.
/// </summary>
public class HeroCurveDay : IRollupRow
{
    /// <summary>
    /// The seconds into the match that stats are sampled at: every 180 seconds for the first 15 minutes, then every 15 minutes.
    /// </summary>
    public static readonly int[] SampleTimes = [180, 360, 540, 720, 900, 1800, 2700, 3600, 4500];

    public int HeroId { get; set; }
    public DateOnly Day { get; set; }
    public short Mode { get; set; }
    public short Tier { get; set; }
    public bool Won { get; set; }
    public int TimeStampS { get; set; }

    /// <summary>
    /// Players with a stat sample at this time. Fewer players reach the later times, because those matches ended sooner.
    /// </summary>
    public int Samples { get; set; }

    public long NetWorth { get; set; }
    public long Level { get; set; }
    public long Kills { get; set; }
    public long Deaths { get; set; }
    public long Assists { get; set; }
    public long PlayerDamage { get; set; }
    public long PlayerDamageTaken { get; set; }
    public long PlayerHealing { get; set; }
    public long WeaponPower { get; set; }
    public long TechPower { get; set; }
    public long MaxHealth { get; set; }

    // Souls by source. Each includes the souls that came from orbs the source dropped.
    public long KillSouls { get; set; }
    public long LaneSouls { get; set; }
    public long NeutralSouls { get; set; }
    public long BossSouls { get; set; }
    public long TreasureSouls { get; set; }
}
