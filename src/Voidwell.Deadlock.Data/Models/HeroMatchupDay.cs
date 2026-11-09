namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// How a hero's players did when a given hero was on the other team, on one UTC day. A game is one player facing one
/// enemy player, so a match adds six games for each hero in it, one per enemy.
/// </summary>
public class HeroMatchupDay : IRollupRow
{
    public int HeroId { get; set; }
    public int EnemyHeroId { get; set; }
    public DateOnly Day { get; set; }
    public short Mode { get; set; }
    public short Tier { get; set; }
    public int Games { get; set; }
    public int Wins { get; set; }
}
