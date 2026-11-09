namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// Results of teams grouped by how many heroes of each archetype they had, on one UTC day. Heroes with no archetype in
/// the game data (at the moment the five support heroes) are counted as supports.
/// </summary>
public class TeamCompositionDay : IRollupRow
{
    public DateOnly Day { get; set; }
    public short Mode { get; set; }
    public short Tier { get; set; }
    public short Brawlers { get; set; }
    public short Assassins { get; set; }
    public short Marksmen { get; set; }
    public short Mystics { get; set; }
    public short Supports { get; set; }

    /// <summary>
    /// Teams, not players: each match adds two.
    /// </summary>
    public int Games { get; set; }
    public int Wins { get; set; }
}
