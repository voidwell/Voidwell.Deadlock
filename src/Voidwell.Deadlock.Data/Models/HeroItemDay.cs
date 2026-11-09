namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// How one hero's players used one shop item on one UTC day, by match mode and badge tier.
/// </summary>
public class HeroItemDay : ItemRollupStats, IRollupRow
{
    public int HeroId { get; set; }
    public DateOnly Day { get; set; }
    public short Mode { get; set; }
    public short Tier { get; set; }
}
