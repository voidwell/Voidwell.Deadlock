using Voidwell.Deadlock.Analytics.Models;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Models;
using Xunit;

namespace Voidwell.Deadlock.Analytics.Test;

public class MatchAnalyticsTest
{
    private static readonly RangeInfo _range = new(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 7), "all", null, null);

    [Theory]
    [InlineData(0, "Core", "core", null)]
    [InlineData(1, "Tier 1, lane 1", "tier1", 1)]
    [InlineData(4, "Tier 1, lane 4", "tier1", 4)]
    [InlineData(5, "Tier 2, lane 1", "tier2", 1)]
    [InlineData(8, "Tier 2, lane 4", "tier2", 4)]
    [InlineData(9, "Titan", "titan", null)]
    [InlineData(10, "Titan shield generator 1", "shieldGenerator", null)]
    [InlineData(11, "Titan shield generator 2", "shieldGenerator", null)]
    [InlineData(12, "Barracks boss, lane 1", "barracks", 1)]
    [InlineData(15, "Barracks boss, lane 4", "barracks", 4)]
    public void ObjectiveInfo_NamesEachBit(int bit, string name, string type, int? lane)
    {
        Assert.Equal((name, type, lane), ObjectiveInfo.Describe(bit));
    }

    [Fact]
    public void ObjectiveInfo_CoversEverySixteenBits()
    {
        for (var bit = 0; bit < MatchObjectiveDay.ObjectiveCount; bit++)
        {
            Assert.NotEqual("unknown", ObjectiveInfo.Describe(bit).Type);
        }
    }

    [Fact]
    public void ToObjectivePoint_ComparesLosingAndKeepingAnObjective()
    {
        // Of 1000 teams, 400 lost the objective and 100 of them won; of the 600 that kept it, 540 won.
        var point = MatchAnalyticsService.ToObjectivePoint(new ObjectiveTotals { Objective = 5, LostGames = 400, LostWins = 100, HeldGames = 600, HeldWins = 540 });

        Assert.Equal("Tier 2, lane 1", point.Name);
        Assert.Equal("tier2", point.Type);
        Assert.Equal(1, point.Lane);
        Assert.Equal(400, point.LostGames);
        Assert.Equal(0.4, point.LostRate, 9);
        Assert.Equal(0.25, point.WinRateWhenLost, 9);
        Assert.Equal(0.9, point.WinRateWhenHeld, 9);
        Assert.True(point.WinRateWhenLostLow < 0.25 && point.WinRateWhenLostHigh > 0.25);
    }

    [Fact]
    public void ToObjectivePoint_WhenTheObjectiveAlwaysFell_HasNoHeldWinRate()
    {
        var point = MatchAnalyticsService.ToObjectivePoint(new ObjectiveTotals { Objective = 1, LostGames = 100, LostWins = 50, HeldGames = 0, HeldWins = 0 });

        Assert.Equal(1.0, point.LostRate, 9);
        Assert.Equal(0, point.HeldGames);
        Assert.Equal(0, point.WinRateWhenHeld);
    }

    [Theory]
    [InlineData(0, 0, "no leavers")]
    [InlineData(1, 0, "1 from your team, 0 from the enemy team")]
    [InlineData(0, 2, "0 from your team, 2 from the enemy team")]
    [InlineData(3, 1, "3+ from your team, 1 from the enemy team")]
    public void LeaverLabel_DescribesTheSituation(int own, int enemy, string expected)
    {
        Assert.Equal(expected, MatchAnalyticsService.LeaverLabel(own, enemy));
    }

    [Fact]
    public void Leavers_SummarizesRatesAndTheEffectOnWinning()
    {
        // 1000 team-matches. A team facing a leaver wins 80% of the time, and a team with one loses 80% of the time.
        var totals = new List<LeaverTotals>
        {
            new() { OwnLeavers = 0, EnemyLeavers = 0, Games = 800, Wins = 400, Players = 4800, LeaverPlayers = 0 },
            new() { OwnLeavers = 1, EnemyLeavers = 0, Games = 100, Wins = 20, Players = 600, LeaverPlayers = 100 },
            new() { OwnLeavers = 0, EnemyLeavers = 1, Games = 100, Wins = 80, Players = 600, LeaverPlayers = 0 }
        };

        var response = MatchAnalyticsService.Leavers(_range, totals);

        Assert.Equal(6000, response.Players);
        Assert.Equal(100, response.Leavers);
        Assert.Equal(100d / 6000, response.LeaverRate, 9);
        Assert.Equal(0.2, response.MatchesWithLeaverRate, 9);
        Assert.Equal(0.2, response.WinRateWithLeaver.Value, 9);
        Assert.Equal(0.8, response.WinRateAgainstLeaver.Value, 9);
        Assert.Equal(3, response.Scenarios.Count);
        Assert.Equal("no leavers", response.Scenarios[0].Label);
    }

    [Fact]
    public void Leavers_WithNoLeaverMatches_HasNoEffectToReport()
    {
        var totals = new List<LeaverTotals> { new() { OwnLeavers = 0, EnemyLeavers = 0, Games = 100, Wins = 50, Players = 600, LeaverPlayers = 0 } };

        var response = MatchAnalyticsService.Leavers(_range, totals);

        Assert.Null(response.WinRateWithLeaver);
        Assert.Null(response.WinRateAgainstLeaver);
        Assert.Equal(0, response.LeaverRate);
        Assert.Equal(0, response.MatchesWithLeaverRate);
    }

    [Fact]
    public void Leavers_WithNoData_DoesNotDivideByZero()
    {
        var response = MatchAnalyticsService.Leavers(_range, []);

        Assert.Equal(0, response.Players);
        Assert.Equal(0, response.LeaverRate);
        Assert.Empty(response.Scenarios);
    }

    [Fact]
    public void Percentile_InterpolatesInsideTheMinute()
    {
        // 100 matches in minute 30 and 100 in minute 31: the median sits at the boundary, the quartiles halfway through each.
        var histogram = new List<DurationTotals> { new() { Minute = 30, Matches = 100 }, new() { Minute = 31, Matches = 100 } };

        Assert.Equal(30.5, MatchAnalyticsService.Percentile(histogram, 0.25), 9);
        Assert.Equal(31.0, MatchAnalyticsService.Percentile(histogram, 0.50), 9);
        Assert.Equal(31.5, MatchAnalyticsService.Percentile(histogram, 0.75), 9);
    }

    [Fact]
    public void Percentile_IgnoresTheOrderOfTheInput()
    {
        var histogram = new List<DurationTotals> { new() { Minute = 40, Matches = 10 }, new() { Minute = 20, Matches = 10 } };

        Assert.Equal(20.1, MatchAnalyticsService.Percentile(histogram, 0.05), 9);
        Assert.Equal(40.5, MatchAnalyticsService.Percentile(histogram, 0.75), 9);
    }

    [Fact]
    public void Percentile_WithNoMatches_IsZero()
    {
        Assert.Equal(0, MatchAnalyticsService.Percentile([], 0.5));
    }
}
