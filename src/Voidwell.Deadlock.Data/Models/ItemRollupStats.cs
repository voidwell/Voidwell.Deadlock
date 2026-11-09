namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// What the item rollups count. A game is one player who bought the item at least once in a match.
/// </summary>
public abstract class ItemRollupStats
{
    public long ItemId { get; set; }

    public int Games { get; set; }
    public int Wins { get; set; }

    /// <summary>
    /// Games where the player still had the item at the end, rather than selling it (usually to upgrade it).
    /// </summary>
    public int HeldGames { get; set; }
    public int HeldWins { get; set; }

    /// <summary>
    /// Sum of the time of each game's first purchase, in seconds.
    /// </summary>
    public long BuyTimeSumS { get; set; }

    // Games and wins split by when the item was first bought: under 10 minutes, 10 to 20, 20 to 30, and 30 or more.
    public int EarlyGames { get; set; }
    public int EarlyWins { get; set; }
    public int MidGames { get; set; }
    public int MidWins { get; set; }
    public int LateGames { get; set; }
    public int LateWins { get; set; }
    public int EndgameGames { get; set; }
    public int EndgameWins { get; set; }
}
