using Xunit;

namespace Voidwell.Deadlock.Analytics.Test;

public class ItemBuildOptimizerTest
{
    // Item indexes
    private static class Items
    {
        public const int A = 0, B = 1, Noise = 2, Late = 3, Starter = 4;
        public const int Count = 5;
    }

    private static readonly ItemBuildOptions _options = new() { MinGames = 100 };

    /// <summary>
    /// Adds <paramref name="games"/> games with the same items, <paramref name="wins"/> of them won.
    /// </summary>
    private static void Add(List<ItemBuildGame> list, int games, int wins, int durationS, params (int Item, int BuyTimeS)[] items)
    {
        for (var i = 0; i < games; i++)
        {
            list.Add(new ItemBuildGame(i < wins, durationS, items.Select(a => a.Item).ToArray(), items.Select(a => a.BuyTimeS).ToArray()));
        }
    }

    /// <summary>
    /// A and B each help a little on their own but a lot together. The noise item is bought in half of every group and changes
    /// nothing. B is bought earlier than A.
    /// </summary>
    private static List<ItemBuildGame> SynergyGames()
    {
        var games = new List<ItemBuildGame>();

        foreach (var noise in new[] { false, true })
        {
            (int, int)[] With(params (int, int)[] items) => noise ? [.. items, (Items.Noise, 600)] : items;

            Add(games, 500, 400, 2400, With((Items.B, 300), (Items.A, 900)));
            Add(games, 500, 200, 2400, With((Items.A, 900)));
            Add(games, 500, 200, 2400, With((Items.B, 300)));
            Add(games, 500, 250, 2400, With());
        }

        return games;
    }

    [Fact]
    public void Find_PicksItemsThatWinTogether_AndReportsTheirSynergy()
    {
        var result = ItemBuildOptimizer.Find(SynergyGames(), Items.Count, _options);

        Assert.Equal([Items.A, Items.B], result.Items.OrderBy(a => a.Pick).Select(a => a.Item));

        var second = result.Items.Single(a => a.Pick == 2);
        Assert.Equal(1000, second.Games);
        Assert.Equal(0.8 - 0.4, second.Lift, 6);

        // On its own B lifts the win rate from 45% to 60%; alongside A, from 40% to 80%.
        Assert.Equal(0.6 - 0.45, second.StandaloneLift, 6);
    }

    [Fact]
    public void Find_StopsWhenNoItemRaisesTheWinRate()
    {
        var result = ItemBuildOptimizer.Find(SynergyGames(), Items.Count, _options);

        Assert.DoesNotContain(result.Items, a => a.Item == Items.Noise);
        Assert.Equal(ItemBuildStopReason.NoBetterItem, result.StopReason);
        Assert.Equal(1000, result.FullBuildGames);
        Assert.Equal(800, result.FullBuildWins);
    }

    [Fact]
    public void Find_ListsItemsInBuyingOrder()
    {
        var result = ItemBuildOptimizer.Find(SynergyGames(), Items.Count, _options);

        // A is chosen first but B is bought first.
        Assert.Equal([Items.B, Items.A], result.Items.Select(a => a.Item));
        Assert.Equal([300, 900], result.Items.Select(a => a.MedianBuyTimeS));
    }

    [Fact]
    public void Find_ComparesLateItemsOnlyWithGamesThatLastedLongEnoughToBuyThem()
    {
        var games = new List<ItemBuildGame>();

        // Short games are lost more often, and none of them last until the late item is bought. Among the long games the
        // late item makes no difference, so it only looks good against every game.
        Add(games, 1000, 300, 1200);
        Add(games, 1000, 600, 2400, (Items.Late, 1800));
        Add(games, 1000, 600, 2400);

        var result = ItemBuildOptimizer.Find(games, Items.Count, _options);

        Assert.Empty(result.Items);
        Assert.Equal(ItemBuildStopReason.NoBetterItem, result.StopReason);
    }

    [Fact]
    public void Find_AddsItemsNearlyEveryoneBuysAsCore_WithoutNarrowingTheGames()
    {
        var games = SynergyGames()
            .Select((a, i) => i % 20 == 0 ? a : a with { Items = [Items.Starter, .. a.Items], BuyTimesS = [0, .. a.BuyTimesS] })
            .ToList();

        var result = ItemBuildOptimizer.Find(games, Items.Count, _options);

        var starter = result.Items.First();
        Assert.Equal(Items.Starter, starter.Item);
        Assert.True(starter.Core);
        Assert.Equal(1, starter.Pick);
        Assert.Equal(3800, starter.Games);

        // A is still judged against every game, not only those with the starter item.
        Assert.Equal(4000, result.Items.Single(a => a.Item == Items.A).CohortGames);
    }

    [Fact]
    public void Find_StopsWhenTooFewGamesRemainToCompare()
    {
        var result = ItemBuildOptimizer.Find(SynergyGames(), Items.Count, new ItemBuildOptions { MinGames = 1500 });

        // After A only 2,000 games remain, so no item can have 1,500 games with it and 1,500 without.
        Assert.Equal([Items.A], result.Items.Select(a => a.Item));
        Assert.Equal(ItemBuildStopReason.NotEnoughGames, result.StopReason);
    }

    [Fact]
    public void Find_StopsAtTheMaximumNumberOfItems()
    {
        var result = ItemBuildOptimizer.Find(SynergyGames(), Items.Count, new ItemBuildOptions { MinGames = 100, MaxItems = 1 });

        Assert.Single(result.Items);
        Assert.Equal(ItemBuildStopReason.MaxItems, result.StopReason);
    }

    [Fact]
    public void Find_WithNoGames_ReturnsAnEmptyBuild()
    {
        var result = ItemBuildOptimizer.Find([], Items.Count, _options);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.FullBuildGames);
        Assert.Equal(ItemBuildStopReason.NotEnoughGames, result.StopReason);
    }

    [Fact]
    public void Difference_GivesASmallSampleAWiderInterval()
    {
        var (smallLift, smallError) = ItemBuildOptimizer.Difference(60, 100, 50, 100);
        var (largeLift, largeError) = ItemBuildOptimizer.Difference(6000, 10000, 5000, 10000);

        Assert.Equal(0.1, smallLift, 6);
        Assert.Equal(0.1, largeLift, 6);
        Assert.Equal(smallError / 10, largeError, 6);
    }

    [Fact]
    public void CriticalZ_WidensTheIntervalForEachItemCompared()
    {
        Assert.Equal(1.96, ItemBuildOptimizer.CriticalZ(1), 2);
        Assert.Equal(2.81, ItemBuildOptimizer.CriticalZ(10), 2);
        Assert.Equal(3.59, ItemBuildOptimizer.CriticalZ(150), 2);
    }

    [Fact]
    public void Find_DoesNotPickItemsThatOnlyLookGoodByChance()
    {
        // 150 items bought at random, none of which affects winning. Compared one at a time, a few would look better than
        // chance; the correction for the number compared keeps them all out.
        var random = new Random(7);
        var games = new List<ItemBuildGame>();

        for (var i = 0; i < 20000; i++)
        {
            var items = Enumerable.Range(0, 150).Where(_ => random.NextDouble() < 0.15).ToArray();
            games.Add(new ItemBuildGame(random.NextDouble() < 0.5, 2400, items, items.Select(_ => random.Next(0, 2400)).ToArray()));
        }

        var result = ItemBuildOptimizer.Find(games, 150, _options);

        Assert.Empty(result.Items);
        Assert.Equal(ItemBuildStopReason.NoBetterItem, result.StopReason);
    }
}
