namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// One item or ability upgrade a player bought. Stored as a TimescaleDB hypertable partitioned on <see cref="StartTime"/>.
/// </summary>
public class MatchPlayerItem
{
    public long MatchId { get; set; }
    public DateTime StartTime { get; set; }
    public int PlayerSlot { get; set; }

    /// <summary>
    /// Position in the player's purchase list, which keeps purchases ordered and the key unique.
    /// </summary>
    public int Seq { get; set; }

    public long ItemId { get; set; }
    public long UpgradeId { get; set; }
    public int GameTimeS { get; set; }

    /// <summary>
    /// Seconds into the match the item was sold, or 0 if it was kept.
    /// </summary>
    public int SoldTimeS { get; set; }
    public int Flags { get; set; }
    public long ImbuedAbilityId { get; set; }
}
