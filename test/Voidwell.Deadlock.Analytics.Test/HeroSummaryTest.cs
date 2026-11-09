using Voidwell.Deadlock.Analytics.Models;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Models;
using Xunit;

namespace Voidwell.Deadlock.Analytics.Test;

public class HeroSummaryTest
{
    // 100 games of 30 minutes each: 55 wins, 7 kills / 5 deaths / 8 assists per game, 30,000 souls per game.
    private static HeroTotals Totals(long statGames = 100, long shotsHit = 600, long shotsMissed = 400)
    {
        return new HeroTotals
        {
            HeroId = 7,
            Games = 100,
            Wins = 55,
            Kills = 700,
            Deaths = 500,
            Assists = 800,
            NetWorth = 3_000_000,
            LastHits = 15_000,
            DurationSumS = 100 * 1800,
            StatGames = statGames,
            PlayerDamage = 6_000_000,
            PlayerDamageTaken = 3_000_000,
            BossDamage = 600_000,
            PlayerHealing = 300_000,
            ShotsHit = shotsHit,
            ShotsMissed = shotsMissed
        };
    }

    [Fact]
    public void Create_ComputesRates()
    {
        var summary = HeroSummary.Create(Totals(), matches: 1000, matchesWithBans: 500, bans: 50, hero: null);

        Assert.Equal(100, summary.Games);
        Assert.Equal(0.55, summary.WinRate, 9);
        Assert.True(summary.WinRateLow < 0.55 && summary.WinRateHigh > 0.55);
        Assert.Equal(100d / 12000, summary.PickRate, 9);
        Assert.Equal(0.1, summary.BanRate.Value, 9);
        Assert.Equal(7, summary.AvgKills, 9);
        Assert.Equal(5, summary.AvgDeaths, 9);
        Assert.Equal(8, summary.AvgAssists, 9);
        Assert.Equal(3, summary.Kda, 9);
        Assert.Equal(30, summary.AvgMatchMinutes, 9);
    }

    [Fact]
    public void Create_ComputesPerMinuteRatesFromMatchLength()
    {
        var summary = HeroSummary.Create(Totals(), 1000, 500, 50, null);

        // 100 games x 30 minutes = 3000 minutes.
        Assert.Equal(1000, summary.SoulsPerMinute, 9);
        Assert.Equal(5, summary.LastHitsPerMinute, 9);
        Assert.Equal(2000, summary.DamagePerMinute.Value, 9);
        Assert.Equal(1000, summary.DamageTakenPerMinute.Value, 9);
        Assert.Equal(200, summary.BossDamagePerMinute.Value, 9);
        Assert.Equal(100, summary.HealingPerMinute.Value, 9);
        Assert.Equal(0.6, summary.Accuracy.Value, 9);
    }

    [Fact]
    public void Create_WhenOnlySomeGamesHaveStats_ScalesTheMinutesToMatch()
    {
        // The damage total covers only half of the games, so it is divided by half of the minutes.
        var summary = HeroSummary.Create(Totals(statGames: 50), 1000, 500, 50, null);

        Assert.Equal(4000, summary.DamagePerMinute.Value, 9);

        // Souls come from the player rows, which cover every game.
        Assert.Equal(1000, summary.SoulsPerMinute, 9);
    }

    [Fact]
    public void Create_WithoutAnyStats_LeavesStatRatesNull()
    {
        var summary = HeroSummary.Create(Totals(statGames: 0, shotsHit: 0, shotsMissed: 0), 1000, 500, 50, null);

        Assert.Null(summary.DamagePerMinute);
        Assert.Null(summary.HealingPerMinute);
        Assert.Null(summary.Accuracy);
    }

    [Fact]
    public void Create_WhenNoMatchRecordedBans_LeavesBanRateNull()
    {
        var summary = HeroSummary.Create(Totals(), 1000, matchesWithBans: 0, bans: 0, hero: null);

        Assert.Null(summary.BanRate);
    }

    [Fact]
    public void Create_WithNoDeaths_DoesNotDivideByZero()
    {
        var totals = Totals();
        totals.Deaths = 0;

        var summary = HeroSummary.Create(totals, 1000, 500, 50, null);

        Assert.Equal(1500, summary.Kda, 9);
    }

    [Fact]
    public void Create_TakesNameAndImagesFromTheHero()
    {
        var hero = new Hero { Id = 7, Name = "Seven", ImageUrl = "card.png", IconUrl = "icon.png" };

        var summary = HeroSummary.Create(Totals(), 1000, 500, 50, hero);

        Assert.Equal("Seven", summary.Name);
        Assert.Equal("card.png", summary.ImageUrl);
        Assert.Equal("icon.png", summary.IconUrl);
    }

    [Fact]
    public void Create_WithNoGames_IsAllZero()
    {
        var summary = HeroSummary.Create(new HeroTotals { HeroId = 1 }, 0, 0, 0, null);

        Assert.Equal(0, summary.WinRate);
        Assert.Equal(0, summary.PickRate);
        Assert.Equal(0, summary.SoulsPerMinute);
        Assert.Null(summary.DamagePerMinute);
    }

    [Fact]
    public void ToFilter_ForRanked_CoversRankedTiers()
    {
        var filter = new AnalyticsQuery { From = new DateOnly(2026, 10, 1), To = new DateOnly(2026, 10, 2), Mode = AnalyticsMode.Ranked }.ToFilter();

        Assert.Equal([(short)MatchMode.Ranked], filter.Modes);
        Assert.Equal(0, filter.MinTier);
        Assert.Equal(11, filter.MaxTier);
    }

    [Fact]
    public void ToFilter_ForRankedWithTiers_UsesThem()
    {
        var filter = new AnalyticsQuery { From = new DateOnly(2026, 10, 1), To = new DateOnly(2026, 10, 2), Mode = AnalyticsMode.Ranked, MinTier = 3, MaxTier = 5 }.ToFilter();

        Assert.Equal(3, filter.MinTier);
        Assert.Equal(5, filter.MaxTier);
    }

    [Fact]
    public void ToFilter_ForUnranked_UsesTheUnrankedTierOnly()
    {
        var filter = new AnalyticsQuery { From = new DateOnly(2026, 10, 1), To = new DateOnly(2026, 10, 2), Mode = AnalyticsMode.Unranked }.ToFilter();

        Assert.Equal([(short)MatchMode.Unranked], filter.Modes);
        Assert.Equal(MatchDay.UnrankedTier, filter.MinTier);
        Assert.Equal(MatchDay.UnrankedTier, filter.MaxTier);
    }

    [Fact]
    public void ToFilter_ForAll_CoversBothModes()
    {
        var filter = new AnalyticsQuery { From = new DateOnly(2026, 10, 1), To = new DateOnly(2026, 10, 2), Mode = AnalyticsMode.All }.ToFilter();

        Assert.Equal(2, filter.Modes.Count);
        Assert.Equal(MatchDay.UnrankedTier, filter.MinTier);
        Assert.Equal(11, filter.MaxTier);
    }

    [Fact]
    public void DayCount_IsInclusive()
    {
        var query = new AnalyticsQuery { From = new DateOnly(2026, 10, 1), To = new DateOnly(2026, 10, 7), Mode = AnalyticsMode.All };

        Assert.Equal(7, query.DayCount);
    }
}
