namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// Results of players who used one community build on one UTC day.
/// </summary>
public class HeroBuildDay : IRollupRow
{
    public int HeroId { get; set; }
    public DateOnly Day { get; set; }
    public short Mode { get; set; }
    public short Tier { get; set; }
    public long BuildId { get; set; }
    public int Games { get; set; }
    public int Wins { get; set; }
}
