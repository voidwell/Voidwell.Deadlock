namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// One shop item across all heroes on one UTC day. The sum of the hero rows, kept so item pages do not have to
/// add up every hero.
/// </summary>
public class ItemDay : ItemRollupStats, IRollupRow
{
    public DateOnly Day { get; set; }
    public short Mode { get; set; }
    public short Tier { get; set; }
}
