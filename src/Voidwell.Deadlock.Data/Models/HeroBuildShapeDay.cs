namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// Results of players grouped by how many of their tier 3 and higher shop items, held at the end, were weapon, spirit
/// and vitality items. Exact item sets are far too varied to compare, but this split is not.
/// </summary>
public class HeroBuildShapeDay : IRollupRow
{
    public int HeroId { get; set; }
    public DateOnly Day { get; set; }
    public short Mode { get; set; }
    public short Tier { get; set; }
    public short WeaponItems { get; set; }
    public short SpiritItems { get; set; }
    public short VitalityItems { get; set; }
    public int Games { get; set; }
    public int Wins { get; set; }
}
