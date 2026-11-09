namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// How a hero's players did when a given hero was on their team, on one UTC day. Each pair is stored in both directions,
/// so one hero's teammates can be read with a single lookup.
/// </summary>
public class HeroSynergyDay : IRollupRow
{
    public int HeroId { get; set; }
    public int AllyHeroId { get; set; }
    public DateOnly Day { get; set; }
    public short Mode { get; set; }
    public short Tier { get; set; }
    public int Games { get; set; }
    public int Wins { get; set; }
}
