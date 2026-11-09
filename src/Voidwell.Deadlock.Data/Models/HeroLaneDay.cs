namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// How a hero's players did against a given hero in the same lane, on one UTC day. Each lane has two players from each
/// team, so every player has two lane opponents. Besides the result of the match, it records the souls lead at nine
/// minutes, which shows who won the lane.
/// </summary>
public class HeroLaneDay : IRollupRow
{
    public int HeroId { get; set; }
    public int EnemyHeroId { get; set; }
    public DateOnly Day { get; set; }
    public short Mode { get; set; }
    public short Tier { get; set; }
    public int Games { get; set; }
    public int Wins { get; set; }

    /// <summary>
    /// Games where both players have a net worth sample at nine minutes.
    /// </summary>
    public int LeadGames { get; set; }

    /// <summary>
    /// Sum, over those games, of the player's net worth minus the lane opponent's at nine minutes.
    /// </summary>
    public long LeadSum { get; set; }
}
