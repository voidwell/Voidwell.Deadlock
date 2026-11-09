namespace Voidwell.Deadlock.Data.Repositories.Models;

/// <summary>
/// One hero's curve rollup rows at one sample time, summed over a filter. Divide a sum by <see cref="Samples"/> for the average.
/// </summary>
public class CurveTotals
{
    public bool Won { get; set; }
    public int TimeStampS { get; set; }
    public long Samples { get; set; }

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
    public long KillSouls { get; set; }
    public long LaneSouls { get; set; }
    public long NeutralSouls { get; set; }
    public long BossSouls { get; set; }
    public long TreasureSouls { get; set; }
}

/// <summary>
/// Matches with a given souls lead at one sample time, summed over a filter.
/// </summary>
public class LeadTotals
{
    public int TimeStampS { get; set; }
    public int LeadBucket { get; set; }
    public long Games { get; set; }
    public long LeaderWins { get; set; }
}
