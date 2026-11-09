using Voidwell.Deadlock.Analytics.Models;
using Voidwell.Deadlock.CollectionStore.Services.Abstractions;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Abstractions;
using Voidwell.Deadlock.Data.Repositories.Models;

namespace Voidwell.Deadlock.Analytics;

public interface IItemAnalyticsService
{
    /// <summary>
    /// Every shop item across all heroes. <paramref name="slot"/> (weapon, spirit or vitality) and
    /// <paramref name="itemTier"/> (1 to 5) limit it to matching items.
    /// </summary>
    Task<ItemStatsResponse> GetItemStatsAsync(AnalyticsQuery query, long minGames, string? slot, int? itemTier);

    /// <summary>
    /// The shop items one hero's players bought. Null when the hero has no games in the query.
    /// </summary>
    Task<HeroItemsResponse?> GetHeroItemsAsync(int heroId, AnalyticsQuery query, long minGames, string? slot, int? itemTier);

    Task<HeroBuildShapesResponse?> GetHeroBuildShapesAsync(int heroId, AnalyticsQuery query, long minGames);

    Task<HeroCommunityBuildsResponse?> GetHeroCommunityBuildsAsync(int heroId, AnalyticsQuery query, long minGames, int limit);

    Task<HeroAbilityOrderResponse?> GetHeroAbilityOrderAsync(int heroId, AnalyticsQuery query);
}

public class ItemAnalyticsService : IItemAnalyticsService
{
    public const int MaxBuildLimit = 200;
    public const int MaxItemTier = 5;

    private static readonly string[] _slots = ["weapon", "spirit", "vitality"];

    private readonly IItemAnalyticsRepository _itemRepository;
    private readonly IHeroAnalyticsRepository _heroRepository;
    private readonly IHeroStore _heroStore;
    private readonly IItemStore _itemStore;

    public ItemAnalyticsService(IItemAnalyticsRepository itemRepository, IHeroAnalyticsRepository heroRepository, IHeroStore heroStore, IItemStore itemStore)
    {
        _itemRepository = itemRepository;
        _heroRepository = heroRepository;
        _heroStore = heroStore;
        _itemStore = itemStore;
    }

    public async Task<ItemStatsResponse> GetItemStatsAsync(AnalyticsQuery query, long minGames, string? slot, int? itemTier)
    {
        var slotFilter = ParseSlot(slot);
        ValidateItemTier(itemTier);

        var filter = query.ToFilter();
        var matches = await _heroRepository.GetMatchTotalsAsync(filter);
        var totals = await _itemRepository.GetItemTotalsAsync(filter);
        var items = await GetItemLookupAsync();

        // Every match has twelve players, so this is the number of players an item could have been bought by.
        var playerCount = matches.Matches * HeroSummary.SlotsPerMatch;

        var summaries = Summarize(totals, items, minGames, slotFilter, itemTier, playerCount, baselineWinRate: 0.5);

        return new ItemStatsResponse(Describe(query), (int)matches.Matches, summaries);
    }

    public async Task<HeroItemsResponse?> GetHeroItemsAsync(int heroId, AnalyticsQuery query, long minGames, string? slot, int? itemTier)
    {
        var slotFilter = ParseSlot(slot);
        ValidateItemTier(itemTier);

        var filter = query.ToFilter();
        var hero = await HeroContexts.LoadAsync(_heroRepository, _heroStore, heroId, filter);
        if (hero == null)
        {
            return null;
        }

        var totals = await _itemRepository.GetHeroItemTotalsAsync(heroId, filter);
        var items = await GetItemLookupAsync();

        var summaries = Summarize(totals, items, minGames, slotFilter, itemTier, hero.Games, hero.WinRate);

        return new HeroItemsResponse(Describe(query), hero, summaries);
    }

    public async Task<HeroBuildShapesResponse?> GetHeroBuildShapesAsync(int heroId, AnalyticsQuery query, long minGames)
    {
        var filter = query.ToFilter();
        var hero = await HeroContexts.LoadAsync(_heroRepository, _heroStore, heroId, filter);
        if (hero == null)
        {
            return null;
        }

        var totals = await _itemRepository.GetBuildShapeTotalsAsync(heroId, filter);
        var allGames = totals.Sum(a => a.Games);

        var shapes = totals
            .Where(a => a.Games >= minGames)
            .Select(a =>
            {
                var (low, high) = Statistics.Wilson(a.Wins, a.Games);

                return new BuildShapePoint(
                    a.WeaponItems,
                    a.SpiritItems,
                    a.VitalityItems,
                    ShapeLabel(a.WeaponItems, a.SpiritItems, a.VitalityItems),
                    (int)a.Games,
                    Statistics.Divide(a.Games, allGames),
                    (int)a.Wins,
                    Statistics.Divide(a.Wins, a.Games),
                    low,
                    high);
            })
            .ToList();

        return new HeroBuildShapesResponse(Describe(query), hero, shapes);
    }

    public async Task<HeroCommunityBuildsResponse?> GetHeroCommunityBuildsAsync(int heroId, AnalyticsQuery query, long minGames, int limit)
    {
        if (limit < 1 || limit > MaxBuildLimit)
        {
            throw new AnalyticsRequestException($"limit must be between 1 and {MaxBuildLimit}.");
        }

        var filter = query.ToFilter();
        var hero = await HeroContexts.LoadAsync(_heroRepository, _heroStore, heroId, filter);
        if (hero == null)
        {
            return null;
        }

        var buildGames = await _itemRepository.GetCommunityBuildGamesAsync(heroId, filter);
        var totals = await _itemRepository.GetCommunityBuildTotalsAsync(heroId, filter, minGames, limit);

        var builds = totals
            .Select(a =>
            {
                var (low, high) = Statistics.Wilson(a.Wins, a.Games);
                var winRate = Statistics.Divide(a.Wins, a.Games);

                return new CommunityBuildPoint(a.BuildId, (int)a.Games, Statistics.Divide(a.Games, buildGames), (int)a.Wins, winRate, low, high, winRate - hero.WinRate);
            })
            .ToList();

        return new HeroCommunityBuildsResponse(Describe(query), hero, Statistics.Divide(buildGames, hero.Games), builds);
    }

    public async Task<HeroAbilityOrderResponse?> GetHeroAbilityOrderAsync(int heroId, AnalyticsQuery query)
    {
        var filter = query.ToFilter();
        var hero = await HeroContexts.LoadAsync(_heroRepository, _heroStore, heroId, filter);
        if (hero == null)
        {
            return null;
        }

        var abilityTotals = await _itemRepository.GetAbilityTotalsAsync(heroId, filter);
        var pickTotals = await _itemRepository.GetAbilityPickTotalsAsync(heroId, filter);
        var items = await GetItemLookupAsync();

        var maxFirstGames = abilityTotals.Sum(a => a.MaxFirstGames);

        var abilities = abilityTotals
            .Select(a => new AbilitySummary(
                a.AbilityId,
                items.GetValueOrDefault(a.AbilityId)?.Name,
                items.GetValueOrDefault(a.AbilityId)?.ImageUrl,
                (int)a.Games,
                Statistics.Divide(a.Games, hero.Games),
                Statistics.Divide(a.Wins, a.Games),
                Statistics.Divide(a.MaxedGames, a.Games),
                a.MaxedGames > 0 ? Statistics.Divide(a.MaxedWins, a.MaxedGames) : null,
                Statistics.Divide(a.MaxFirstGames, maxFirstGames),
                a.MaxFirstGames > 0 ? Statistics.Divide(a.MaxFirstWins, a.MaxFirstGames) : null,
                Statistics.Divide(a.UnlockTimeSumS, a.Games) / 60))
            .OrderByDescending(a => a.MaxFirstRate)
            .ThenByDescending(a => a.Games)
            .ToList();

        var picks = pickTotals
            .GroupBy(a => a.PickIndex)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var pickGames = g.Sum(a => a.Games);

                var options = g
                    .OrderByDescending(a => a.Games)
                    .Select(a => new AbilityPickOption(
                        a.AbilityId,
                        items.GetValueOrDefault(a.AbilityId)?.Name,
                        (int)a.Games,
                        Statistics.Divide(a.Games, pickGames),
                        Statistics.Divide(a.Wins, a.Games)))
                    .ToList();

                return new AbilityPickPoint(g.Key, options);
            })
            .ToList();

        return new HeroAbilityOrderResponse(Describe(query), hero, abilities, picks, AbilityOrder.Suggest(picks));
    }

    public static string ShapeLabel(int weapon, int spirit, int vitality)
    {
        return $"{weapon}W/{spirit}S/{vitality}V";
    }

    private static List<ItemSummary> Summarize(List<ItemTotals> totals, Dictionary<long, Item> items, long minGames,
        string? slot, int? itemTier, long playerCount, double baselineWinRate)
    {
        return totals
            .Where(a => a.Games >= minGames)
            .Select(a => (Totals: a, Item: items.GetValueOrDefault(a.ItemId)))
            .Where(a => slot == null || a.Item?.SlotType == slot)
            .Where(a => itemTier == null || a.Item?.Tier == itemTier)
            .Select(a => ItemSummary.Create(a.Totals, a.Item, playerCount, baselineWinRate))
            .OrderByDescending(a => a.Games)
            .ToList();
    }

    private static string? ParseSlot(string? slot)
    {
        if (string.IsNullOrWhiteSpace(slot))
        {
            return null;
        }

        var normalized = slot.Trim().ToLowerInvariant();

        return _slots.Contains(normalized)
            ? normalized
            : throw new AnalyticsRequestException("slot must be weapon, spirit or vitality.");
    }

    private static void ValidateItemTier(int? itemTier)
    {
        if (itemTier is < 1 or > MaxItemTier)
        {
            throw new AnalyticsRequestException($"itemTier must be between 1 and {MaxItemTier}.");
        }
    }

    private static RangeInfo Describe(AnalyticsQuery query)
    {
        return new RangeInfo(query.From, query.To, query.Mode.ToString().ToLowerInvariant(), query.MinTier, query.MaxTier);
    }

    private async Task<Dictionary<long, Item>> GetItemLookupAsync()
    {
        return (await _itemStore.GetAllItems() ?? []).ToDictionary(a => a.Id);
    }
}
