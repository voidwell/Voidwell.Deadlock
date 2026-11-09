using Voidwell.Deadlock.Data.Repositories.Models;

namespace Voidwell.Deadlock.Data.Repositories.Abstractions;

/// <summary>
/// Reads the item, build and ability rollups. Every method sums the daily rows that match the filter.
/// </summary>
public interface IItemAnalyticsRepository
{
    /// <summary>
    /// Every shop item across all heroes, one row per item.
    /// </summary>
    Task<List<ItemTotals>> GetItemTotalsAsync(AnalyticsFilter filter);

    /// <summary>
    /// Every shop item one hero's players bought, one row per item.
    /// </summary>
    Task<List<ItemTotals>> GetHeroItemTotalsAsync(int heroId, AnalyticsFilter filter);

    Task<List<BuildShapeTotals>> GetBuildShapeTotalsAsync(int heroId, AnalyticsFilter filter);

    /// <summary>
    /// The hero's most played community builds with at least <paramref name="minGames"/> games, most played first.
    /// </summary>
    Task<List<CommunityBuildTotals>> GetCommunityBuildTotalsAsync(int heroId, AnalyticsFilter filter, long minGames, int limit);

    /// <summary>
    /// How many of the hero's games used any community build.
    /// </summary>
    Task<long> GetCommunityBuildGamesAsync(int heroId, AnalyticsFilter filter);

    Task<List<AbilityTotals>> GetAbilityTotalsAsync(int heroId, AnalyticsFilter filter);

    Task<List<AbilityPickTotals>> GetAbilityPickTotalsAsync(int heroId, AnalyticsFilter filter);
}
