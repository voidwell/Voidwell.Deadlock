namespace Voidwell.Deadlock.Data.Repositories.Models;

/// <summary>
/// Match rollup rows summed over a filter. <see cref="Day"/> or <see cref="Tier"/> is set when the sum was grouped by it.
/// </summary>
public class MatchTotals
{
    public DateOnly? Day { get; set; }
    public int? Tier { get; set; }
    public int? Mode { get; set; }

    public long Matches { get; set; }
    public long MatchesWithBans { get; set; }
    public long WinsTeam0 { get; set; }
    public long DurationSumS { get; set; }
}
