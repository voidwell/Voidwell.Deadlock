namespace Voidwell.Deadlock.Data.Repositories.Models;

/// <summary>
/// One player's shop items in one match, from <c>hero_item_build</c>.
/// </summary>
public class HeroItemBuildRecord
{
    public bool Won { get; init; }
    public int DurationS { get; init; }
    public long[] ItemIds { get; init; } = [];
    public int[] BuyTimesS { get; init; } = [];
}
