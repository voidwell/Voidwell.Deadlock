namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// How teams fared by how many of their own players and how many of the enemy team's players left the match, on one UTC
/// day. A match adds a row for each team.
/// </summary>
public class MatchLeaverDay : IRollupRow
{
    /// <summary>
    /// Counts of this many leavers or more share the last row.
    /// </summary>
    public const int MaxLeavers = 3;

    public DateOnly Day { get; set; }
    public short Mode { get; set; }
    public short Tier { get; set; }
    public short OwnLeavers { get; set; }
    public short EnemyLeavers { get; set; }

    /// <summary>
    /// Teams in this situation, and how many of them won.
    /// </summary>
    public int Games { get; set; }
    public int Wins { get; set; }

    /// <summary>
    /// Players on those teams, and how many of them left.
    /// </summary>
    public long Players { get; set; }
    public long LeaverPlayers { get; set; }
}
