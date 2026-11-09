namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// How teams fared when an objective was destroyed or left standing, on one UTC day. Each match counts once for each
/// team. The game stores which objectives are still standing at the end of a match as a bit for each objective, so this
/// says whether an objective was destroyed by the end, not when.
/// </summary>
public class MatchObjectiveDay : IRollupRow
{
    /// <summary>
    /// The number of objective bits the game stores for a team.
    /// </summary>
    public const int ObjectiveCount = 16;

    public DateOnly Day { get; set; }
    public short Mode { get; set; }
    public short Tier { get; set; }

    /// <summary>
    /// The objective, as its bit number from 0 to 15.
    /// </summary>
    public short Objective { get; set; }

    /// <summary>
    /// Teams that had the objective destroyed by the end of the match, and how many of those won.
    /// </summary>
    public int LostGames { get; set; }
    public int LostWins { get; set; }

    /// <summary>
    /// Teams that still had the objective standing at the end of the match, and how many of those won.
    /// </summary>
    public int HeldGames { get; set; }
    public int HeldWins { get; set; }
}
