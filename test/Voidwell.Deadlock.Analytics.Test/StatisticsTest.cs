using Xunit;

namespace Voidwell.Deadlock.Analytics.Test;

public class StatisticsTest
{
    [Fact]
    public void Wilson_ForEvenSplit_IsSymmetricAroundOneHalf()
    {
        var (low, high) = Statistics.Wilson(50, 100);

        Assert.Equal(0.4038, low, 3);
        Assert.Equal(0.5962, high, 3);
    }

    [Fact]
    public void Wilson_WithNoSuccesses_StaysInsideZeroToOne()
    {
        var (low, high) = Statistics.Wilson(0, 10);

        Assert.Equal(0, low, 9);
        Assert.Equal(0.2775, high, 3);
    }

    [Fact]
    public void Wilson_WithNoGames_IsZero()
    {
        Assert.Equal((0d, 0d), Statistics.Wilson(0, 0));
    }

    [Fact]
    public void Wilson_NarrowsAsGamesGrow()
    {
        var (smallLow, smallHigh) = Statistics.Wilson(55, 100);
        var (largeLow, largeHigh) = Statistics.Wilson(5500, 10000);

        Assert.True(largeHigh - largeLow < smallHigh - smallLow);
    }

    [Fact]
    public void TwoProportionZ_ForEqualRates_IsZero()
    {
        Assert.Equal(0, Statistics.TwoProportionZ(50, 100, 500, 1000), 9);
    }

    [Fact]
    public void TwoProportionZ_ForSixtyVersusFortyPercent_IsAboutTwoPointEight()
    {
        Assert.Equal(2.828, Statistics.TwoProportionZ(60, 100, 40, 100), 3);
        Assert.Equal(-2.828, Statistics.TwoProportionZ(40, 100, 60, 100), 3);
    }

    [Fact]
    public void TwoProportionZ_WithEmptyGroup_IsZero()
    {
        Assert.Equal(0, Statistics.TwoProportionZ(0, 0, 5, 10));
    }

    [Fact]
    public void Divide_ByZero_IsZero()
    {
        Assert.Equal(0, Statistics.Divide(5, 0));
    }

    [Theory]
    [InlineData(0.60, "S")]
    [InlineData(0.53, "S")]
    [InlineData(0.5299, "A")]
    [InlineData(0.51, "A")]
    [InlineData(0.5099, "B")]
    [InlineData(0.49, "B")]
    [InlineData(0.4899, "C")]
    [InlineData(0.47, "C")]
    [InlineData(0.4699, "D")]
    [InlineData(0.30, "D")]
    public void TierRating_SortsByWinRate(double winRate, string expected)
    {
        Assert.Equal(expected, TierRating.FromWinRate(winRate));
    }
}
