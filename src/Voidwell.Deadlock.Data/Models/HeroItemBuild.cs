namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// The shop items one player bought in one match, for finding which items win together. Unlike the other rollups this
/// keeps one row per player, because item combinations are far too many to count in advance. Stored as a TimescaleDB
/// hypertable partitioned on <see cref="Day"/> and compressed by hero.
/// </summary>
public class HeroItemBuild : IRollupRow
{
    public int HeroId { get; set; }
    public DateOnly Day { get; set; }
    public short Mode { get; set; }
    public short Tier { get; set; }
    public long MatchId { get; set; }
    public int PlayerSlot { get; set; }

    public bool Won { get; set; }
    public int DurationS { get; set; }

    /// <summary>
    /// Each shop item the player bought, once, in the order they first bought them.
    /// </summary>
    public long[] ItemIds { get; set; } = [];

    /// <summary>
    /// Seconds into the match each item in <see cref="ItemIds"/> was first bought.
    /// </summary>
    public int[] BuyTimesS { get; set; } = [];
}
