using Voidwell.Deadlock.Analytics.Models;
using Voidwell.Deadlock.CollectionStore.Services.Abstractions;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Abstractions;

namespace Voidwell.Deadlock.Analytics;

public interface IItemBuildService
{
    /// <summary>
    /// The shop items that gave the hero's players the best odds of winning together in the query, in the order to buy
    /// them. Null when the hero has no games in the query.
    /// </summary>
    Task<HeroItemBuildResponse?> GetHeroItemBuildAsync(int heroId, AnalyticsQuery query, int minGames, int maxItems,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Suggests a hero's item build from every game in a period. See <see cref="ItemBuildOptimizer"/> for how.
/// </summary>
public class ItemBuildService : IItemBuildService
{
    /// <summary>
    /// The longest period a build can be worked out from. Every game in it is loaded, about 6,000 a day for a popular hero,
    /// which for 60 days is around 100 MB while the request runs.
    /// </summary>
    public const int MaxDays = 60;

    /// <summary>
    /// Twelve item slots and four flex slots.
    /// </summary>
    public const int MaxItems = 16;

    public const int LowestMinGames = 10;

    private readonly IItemAnalyticsRepository _itemRepository;
    private readonly IHeroAnalyticsRepository _heroRepository;
    private readonly IHeroStore _heroStore;
    private readonly IItemStore _itemStore;

    public ItemBuildService(IItemAnalyticsRepository itemRepository, IHeroAnalyticsRepository heroRepository, IHeroStore heroStore, IItemStore itemStore)
    {
        _itemRepository = itemRepository;
        _heroRepository = heroRepository;
        _heroStore = heroStore;
        _itemStore = itemStore;
    }

    public async Task<HeroItemBuildResponse?> GetHeroItemBuildAsync(int heroId, AnalyticsQuery query, int minGames, int maxItems,
        CancellationToken cancellationToken = default)
    {
        if (maxItems < 1 || maxItems > MaxItems)
        {
            throw new AnalyticsRequestException($"maxItems must be between 1 and {MaxItems}.");
        }

        if (minGames < LowestMinGames)
        {
            throw new AnalyticsRequestException($"minGames must be at least {LowestMinGames}.");
        }

        if (query.DayCount > MaxDays)
        {
            throw new AnalyticsRequestException($"An item build covers at most {MaxDays} days.");
        }

        var filter = query.ToFilter();
        var hero = await HeroContexts.LoadAsync(_heroRepository, _heroStore, heroId, filter);
        if (hero == null)
        {
            return null;
        }

        // Item ids are stored as the game's large ids; the optimizer works with small consecutive indexes.
        var itemIds = new List<long>();
        var itemIndexes = new Dictionary<long, int>();
        var games = new List<ItemBuildGame>();

        await foreach (var record in _itemRepository.GetHeroItemBuildsAsync(heroId, filter, cancellationToken))
        {
            var items = new int[record.ItemIds.Length];

            for (var i = 0; i < items.Length; i++)
            {
                if (!itemIndexes.TryGetValue(record.ItemIds[i], out var index))
                {
                    index = itemIds.Count;
                    itemIds.Add(record.ItemIds[i]);
                    itemIndexes.Add(record.ItemIds[i], index);
                }

                items[i] = index;
            }

            games.Add(new ItemBuildGame(record.Won, record.DurationS, items, record.BuyTimesS));
        }

        var result = ItemBuildOptimizer.Find(games, itemIds.Count, new ItemBuildOptions { MaxItems = maxItems, MinGames = minGames });
        var itemLookup = (await _itemStore.GetAllItems() ?? []).ToDictionary(a => a.Id);
        var wins = games.Count(a => a.Won);

        var steps = result.Items
            .Select((pick, i) => ToStep(pick, i + 1, itemIds[pick.Item], itemLookup.GetValueOrDefault(itemIds[pick.Item]), firstChosen: !pick.Core && result.Items.All(a => a.Core || a.Pick >= pick.Pick)))
            .ToList();

        double? fullBuildWinRate = null, fullBuildLow = null, fullBuildHigh = null;
        if (result.FullBuildGames > 0)
        {
            fullBuildWinRate = Statistics.Divide(result.FullBuildWins, result.FullBuildGames);
            (fullBuildLow, fullBuildHigh) = Statistics.Wilson(result.FullBuildWins, result.FullBuildGames);
        }

        return new HeroItemBuildResponse(
            new RangeInfo(query.From, query.To, query.Mode.ToString().ToLowerInvariant(), query.MinTier, query.MaxTier),
            hero,
            games.Count,
            Statistics.Divide(wins, games.Count),
            steps,
            result.FullBuildGames,
            fullBuildWinRate,
            fullBuildLow,
            fullBuildHigh,
            StopReasonName(result.StopReason));
    }

    private static ItemBuildStep ToStep(ItemBuildPick pick, int order, long itemId, Item? item, bool firstChosen)
    {
        // A core item bought in every game has nothing to be compared with.
        var measurable = pick.GamesWithout > 0;

        return new ItemBuildStep(
            order,
            itemId,
            item?.Name,
            item?.ImageUrl,
            item?.SlotType,
            item?.Tier,
            item?.Cost,
            pick.MedianBuyTimeS / 60.0,
            pick.Core,
            pick.Pick,
            pick.CohortGames,
            pick.Games,
            Statistics.Divide(pick.Games, pick.CohortGames),
            Statistics.Divide(pick.Wins, pick.Games),
            pick.GamesWithout,
            Statistics.Divide(pick.WinsWithout, pick.GamesWithout),
            measurable ? pick.Lift : null,
            measurable ? pick.LiftLow : null,
            measurable ? pick.StandaloneLift : null,
            pick.Core || firstChosen ? null : pick.Lift - pick.StandaloneLift);
    }

    private static string StopReasonName(ItemBuildStopReason reason)
    {
        var name = reason.ToString();
        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}
