using Voidwell.Deadlock.Analytics.Models;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Models;
using Xunit;

namespace Voidwell.Deadlock.Analytics.Test;

public class ItemSummaryTest
{
    // 200 buyers: 110 won. 50 kept the item (30 of them won). They first bought it 15 minutes in on average.
    private static ItemTotals Totals()
    {
        return new ItemTotals
        {
            ItemId = 1548066885,
            Games = 200,
            Wins = 110,
            HeldGames = 50,
            HeldWins = 30,
            BuyTimeSumS = 200 * 900,
            EarlyGames = 20,
            EarlyWins = 8,
            MidGames = 100,
            MidWins = 55,
            LateGames = 60,
            LateWins = 36,
            EndgameGames = 20,
            EndgameWins = 11
        };
    }

    [Fact]
    public void Create_ComputesRates()
    {
        var summary = ItemSummary.Create(Totals(), null, playerCount: 1000, baselineWinRate: 0.5);

        Assert.Equal(200, summary.Games);
        Assert.Equal(110, summary.Wins);
        Assert.Equal(0.55, summary.WinRate, 9);
        Assert.True(summary.WinRateLow < 0.55 && summary.WinRateHigh > 0.55);
        Assert.Equal(0.2, summary.BuyRate, 9);
        Assert.Equal(0.25, summary.KeptRate, 9);
        Assert.Equal(0.6, summary.KeptWinRate.Value, 9);
        Assert.Equal(15, summary.AvgFirstBuyMinutes, 9);
    }

    [Fact]
    public void Create_LiftIsAgainstTheBaseline()
    {
        var summary = ItemSummary.Create(Totals(), null, 1000, baselineWinRate: 0.52);

        Assert.Equal(0.03, summary.WinRateLift, 9);
    }

    [Fact]
    public void Create_SplitsBuyersByWhenTheyBought()
    {
        var summary = ItemSummary.Create(Totals(), null, 1000, 0.5);

        Assert.Equal(["early", "mid", "late", "endgame"], summary.Timing.Select(a => a.Bucket));
        Assert.Equal([20, 100, 60, 20], summary.Timing.Select(a => a.Games));
        Assert.Equal(1.0, summary.Timing.Sum(a => a.Share), 9);
        Assert.Equal(0.4, summary.Timing[0].WinRate, 9);
        Assert.Equal(0.6, summary.Timing[2].WinRate, 9);
    }

    [Fact]
    public void Create_TakesDetailsFromTheReferenceItem()
    {
        var item = new Item { Id = 1548066885, Name = "Extended Magazine", ImageUrl = "mag.png", SlotType = "weapon", Tier = 1, Cost = 800 };

        var summary = ItemSummary.Create(Totals(), item, 1000, 0.5);

        Assert.Equal("Extended Magazine", summary.Name);
        Assert.Equal("mag.png", summary.ImageUrl);
        Assert.Equal("weapon", summary.SlotType);
        Assert.Equal(1, summary.Tier);
        Assert.Equal(800, summary.Cost);
    }

    [Fact]
    public void Create_WithoutAReferenceItem_StillReturnsTheStats()
    {
        var summary = ItemSummary.Create(Totals(), null, 1000, 0.5);

        Assert.Null(summary.Name);
        Assert.Null(summary.SlotType);
        Assert.Equal(1548066885, summary.ItemId);
    }

    [Fact]
    public void Create_WhenNobodyKeptTheItem_HasNoKeptWinRate()
    {
        var totals = Totals();
        totals.HeldGames = 0;
        totals.HeldWins = 0;

        var summary = ItemSummary.Create(totals, null, 1000, 0.5);

        Assert.Equal(0, summary.KeptRate);
        Assert.Null(summary.KeptWinRate);
    }

    [Fact]
    public void Create_WithNoGames_IsAllZero()
    {
        var summary = ItemSummary.Create(new ItemTotals { ItemId = 5 }, null, 0, 0.5);

        Assert.Equal(0, summary.WinRate);
        Assert.Equal(0, summary.BuyRate);
        Assert.Equal(0, summary.AvgFirstBuyMinutes);
        Assert.All(summary.Timing, a => Assert.Equal(0, a.Games));
    }

    [Theory]
    [InlineData(4, 2, 1, "4W/2S/1V")]
    [InlineData(0, 0, 0, "0W/0S/0V")]
    public void ShapeLabel_ListsTheSlotCounts(int weapon, int spirit, int vitality, string expected)
    {
        Assert.Equal(expected, ItemAnalyticsService.ShapeLabel(weapon, spirit, vitality));
    }
}
