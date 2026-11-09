namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// Matches that started in one hour of the (UTC) day, on one UTC day, split by match mode and badge tier.
/// </summary>
public class MatchHourDay : IRollupRow
{
    public DateOnly Day { get; set; }
    public short Mode { get; set; }
    public short Tier { get; set; }

    /// <summary>
    /// The UTC hour the matches started in, 0 to 23.
    /// </summary>
    public short Hour { get; set; }
    public int Matches { get; set; }
}
