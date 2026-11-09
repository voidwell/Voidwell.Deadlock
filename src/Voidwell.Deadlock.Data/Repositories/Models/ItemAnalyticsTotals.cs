namespace Voidwell.Deadlock.Data.Repositories.Models;

/// <summary>
/// One shop item's item rollup rows summed over a filter. See <c>ItemRollupStats</c> for what the counts mean.
/// </summary>
public class ItemTotals
{
    public long ItemId { get; set; }
    public long Games { get; set; }
    public long Wins { get; set; }
    public long HeldGames { get; set; }
    public long HeldWins { get; set; }
    public long BuyTimeSumS { get; set; }
    public long EarlyGames { get; set; }
    public long EarlyWins { get; set; }
    public long MidGames { get; set; }
    public long MidWins { get; set; }
    public long LateGames { get; set; }
    public long LateWins { get; set; }
    public long EndgameGames { get; set; }
    public long EndgameWins { get; set; }
}

/// <summary>
/// Games and wins of players whose held tier 3 and higher items split a given way across the three slots.
/// </summary>
public class BuildShapeTotals
{
    public int WeaponItems { get; set; }
    public int SpiritItems { get; set; }
    public int VitalityItems { get; set; }
    public long Games { get; set; }
    public long Wins { get; set; }
}

public class CommunityBuildTotals
{
    public long BuildId { get; set; }
    public long Games { get; set; }
    public long Wins { get; set; }
}

public class AbilityTotals
{
    public long AbilityId { get; set; }
    public long Games { get; set; }
    public long Wins { get; set; }
    public long MaxedGames { get; set; }
    public long MaxedWins { get; set; }
    public long MaxFirstGames { get; set; }
    public long MaxFirstWins { get; set; }
    public long UnlockTimeSumS { get; set; }
}

public class AbilityPickTotals
{
    public int PickIndex { get; set; }
    public long AbilityId { get; set; }
    public long Games { get; set; }
    public long Wins { get; set; }
}
