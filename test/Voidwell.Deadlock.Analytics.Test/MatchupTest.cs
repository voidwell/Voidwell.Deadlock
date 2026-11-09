using Voidwell.Deadlock.Analytics.Models;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Models;
using Xunit;

namespace Voidwell.Deadlock.Analytics.Test;

public class MatchupTest
{
    [Fact]
    public void Expected_AgainstAnEnemy_AveragesMyWinRateAndTheirLossRate()
    {
        // A 52% hero against a 48% hero would be expected to win 52% of the time.
        Assert.Equal(0.52, PairSummary.Expected(0.52, 0.48, opponent: true), 9);

        // Against a strong 56% hero, the same hero would be expected to do worse.
        Assert.Equal(0.48, PairSummary.Expected(0.52, 0.56, opponent: true), 9);
    }

    [Fact]
    public void Expected_WithATeammate_AveragesBothWinRates()
    {
        Assert.Equal(0.51, PairSummary.Expected(0.52, 0.50, opponent: false), 9);
    }

    [Fact]
    public void Create_MeasuresTheAdvantageAgainstWhatWasExpected()
    {
        // The hero wins 60 of 100 against an enemy that both heroes' overall rates predict at 50%.
        var totals = new PairTotals { OtherHeroId = 9, Games = 100, Wins = 60 };

        var summary = PairSummary.Create(totals, new Hero { Id = 9, Name = "Nine", IconUrl = "nine.png" }, 0.5, 0.5, opponent: true);

        Assert.Equal(9, summary.HeroId);
        Assert.Equal("Nine", summary.Name);
        Assert.Equal("nine.png", summary.IconUrl);
        Assert.Equal(0.6, summary.WinRate, 9);
        Assert.Equal(0.5, summary.ExpectedWinRate, 9);
        Assert.Equal(0.1, summary.Advantage, 9);
        Assert.True(summary.WinRateLow < 0.6 && summary.WinRateHigh > 0.6);
        Assert.Null(summary.AvgLeadAtNineMinutes);
    }

    [Fact]
    public void Create_AveragesTheLaneLeadOverGamesWithBothSamples()
    {
        // 80 of the 100 games had both nine-minute samples, and the player led by 40,000 souls in total.
        var totals = new PairTotals { OtherHeroId = 9, Games = 100, Wins = 50, LeadGames = 80, LeadSum = 40_000 };

        var summary = PairSummary.Create(totals, null, 0.5, 0.5, opponent: true);

        Assert.Equal(500, summary.AvgLeadAtNineMinutes.Value, 9);
    }

    [Fact]
    public void Create_ForANegativeLead_KeepsTheSign()
    {
        var totals = new PairTotals { OtherHeroId = 9, Games = 10, Wins = 4, LeadGames = 10, LeadSum = -2500 };

        Assert.Equal(-250, PairSummary.Create(totals, null, 0.5, 0.5, true).AvgLeadAtNineMinutes.Value, 9);
    }

    [Fact]
    public void Create_WithoutAReferenceHero_StillReturnsTheStats()
    {
        var summary = PairSummary.Create(new PairTotals { OtherHeroId = 9, Games = 10, Wins = 5 }, null, 0.5, 0.5, false);

        Assert.Null(summary.Name);
        Assert.Equal(10, summary.Games);
    }

    [Fact]
    public void CompositionLabel_ListsTheArchetypesPresent()
    {
        var label = MatchupAnalyticsService.CompositionLabel(new CompositionTotals { Brawlers = 2, Assassins = 1, Mystics = 2, Supports = 1 });

        Assert.Equal("2 brawler, 1 assassin, 2 mystic, 1 support", label);
    }

    [Fact]
    public void CompositionLabel_LeavesOutArchetypesWithNone()
    {
        var label = MatchupAnalyticsService.CompositionLabel(new CompositionTotals { Marksmen = 3, Mystics = 3 });

        Assert.Equal("3 marksman, 3 mystic", label);
    }

    [Fact]
    public void CompositionLabel_ForAnEmptyTeam_IsNone()
    {
        Assert.Equal("none", MatchupAnalyticsService.CompositionLabel(new CompositionTotals()));
    }
}
