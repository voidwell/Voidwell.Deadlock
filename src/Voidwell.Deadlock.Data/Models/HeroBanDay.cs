namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// How often a hero was banned in matches that started on one UTC day. Rollup of <see cref="Match.BannedHeroIds"/>.
/// </summary>
public class HeroBanDay : IRollupRow
{
    public DateOnly Day { get; set; }
    public short Mode { get; set; }
    public short Tier { get; set; }
    public int HeroId { get; set; }
    public int Bans { get; set; }
}
