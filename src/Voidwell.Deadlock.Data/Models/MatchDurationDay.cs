namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// Matches of one length on one UTC day, split by match mode and badge tier. Lengths are whole minutes, with everything
/// from <see cref="MaxMinute"/> up in the last row.
/// </summary>
public class MatchDurationDay : IRollupRow
{
    public const int MaxMinute = 120;

    public DateOnly Day { get; set; }
    public short Mode { get; set; }
    public short Tier { get; set; }

    /// <summary>
    /// The match length in whole minutes, rounded down.
    /// </summary>
    public short Minute { get; set; }
    public int Matches { get; set; }
}
