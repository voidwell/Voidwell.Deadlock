namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// How often players spent their Nth ability point on one ability, on one UTC day.
/// </summary>
public class HeroAbilityPickDay : IRollupRow
{
    public int HeroId { get; set; }
    public DateOnly Day { get; set; }
    public short Mode { get; set; }
    public short Tier { get; set; }

    /// <summary>
    /// Which ability point this was: 1 for the first, up to 16.
    /// </summary>
    public short PickIndex { get; set; }
    public long AbilityId { get; set; }
    public int Games { get; set; }
    public int Wins { get; set; }
}
