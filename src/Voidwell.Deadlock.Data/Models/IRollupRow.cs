namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// The columns every rollup row shares: a UTC day, a match mode and a badge tier.
/// </summary>
public interface IRollupRow
{
    DateOnly Day { get; set; }
    short Mode { get; set; }
    short Tier { get; set; }
}
