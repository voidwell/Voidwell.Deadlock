using Voidwell.Deadlock.Analytics.Models;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Models;
using Xunit;

namespace Voidwell.Deadlock.Analytics.Test;

public class CurveTest
{
    // 100 players at 15 minutes who had 12,000 souls on average.
    private static CurveTotals Totals(long samples = 100)
    {
        return new CurveTotals
        {
            TimeStampS = 900,
            Samples = samples,
            NetWorth = 12_000 * samples,
            Level = 18 * samples,
            Kills = 2 * samples,
            Deaths = 3 * samples,
            Assists = 4 * samples,
            PlayerDamage = 7_000 * samples,
            PlayerDamageTaken = 6_000 * samples,
            PlayerHealing = 1_500 * samples,
            WeaponPower = 36 * samples,
            TechPower = 58 * samples,
            MaxHealth = 1_900 * samples,
            KillSouls = 2_000 * samples,
            LaneSouls = 6_000 * samples,
            NeutralSouls = 1_000 * samples,
            BossSouls = 800 * samples,
            TreasureSouls = 200 * samples
        };
    }

    [Fact]
    public void CurveStats_AveragesTheSums()
    {
        var stats = CurveStats.Create(Totals());

        Assert.Equal(100, stats.Samples);
        Assert.Equal(12_000, stats.NetWorth, 9);
        Assert.Equal(18, stats.Level, 9);
        Assert.Equal(2, stats.Kills, 9);
        Assert.Equal(7_000, stats.PlayerDamage, 9);
        Assert.Equal(1_500, stats.PlayerHealing, 9);
        Assert.Equal(58, stats.TechPower, 9);
    }

    [Fact]
    public void Merge_AddsWinnersAndLosers()
    {
        var winners = Totals(60);
        var losers = Totals(40);
        losers.NetWorth = 10_000 * 40;

        var all = CurveAnalyticsService.Merge([winners, losers]);

        Assert.Equal(100, all.Samples);
        Assert.Equal(900, all.TimeStampS);
        Assert.Equal((12_000 * 60) + (10_000 * 40), all.NetWorth);
        Assert.Equal((2 * 60) + (2 * 40), all.Kills);
    }

    [Fact]
    public void SoulsSources_SplitsNetWorthAndShowsTheRestAsOther()
    {
        var point = CurveAnalyticsService.SoulsSources(15, Totals());

        Assert.Equal(15, point.Minute);
        Assert.Equal(100, point.Samples);
        Assert.Equal(12_000, point.NetWorth, 9);
        Assert.Equal(["kills", "laneCreeps", "neutrals", "bosses", "treasure", "other"], point.Sources.Select(a => a.Source));

        // 2000 + 6000 + 1000 + 800 + 200 = 10,000 of the 12,000, so 2,000 is not explained by the named sources.
        Assert.Equal(6_000, point.Sources[1].Souls, 9);
        Assert.Equal(0.5, point.Sources[1].Share, 9);
        Assert.Equal(2_000, point.Sources[5].Souls, 9);
        Assert.Equal(1.0, point.Sources.Sum(a => a.Share), 9);
    }

    [Fact]
    public void SoulsSources_WithNoSamples_DoesNotDivideByZero()
    {
        var point = CurveAnalyticsService.SoulsSources(3, new CurveTotals());

        Assert.Equal(0, point.NetWorth);
        Assert.All(point.Sources, a => Assert.Equal(0, a.Share));
    }

    [Fact]
    public void LeadMinute_TotalsAllBucketsAndMirrorsTheComebackRate()
    {
        var buckets = new List<LeadTotals>
        {
            new() { TimeStampS = 540, LeadBucket = 0, Games = 100, LeaderWins = 55 },
            new() { TimeStampS = 540, LeadBucket = 3, Games = 100, LeaderWins = 70 },
            new() { TimeStampS = 540, LeadBucket = 6, Games = 100, LeaderWins = 90 }
        };

        var minute = CurveAnalyticsService.LeadMinute(9, buckets, minGames: 1);

        Assert.Equal(9, minute.Minute);
        Assert.Equal(300, minute.Games);
        Assert.Equal(215d / 300, minute.LeaderWinRate, 9);
        Assert.Equal(1 - (215d / 300), minute.ComebackRate, 9);
        Assert.True(minute.LeaderWinRateLow < minute.LeaderWinRate && minute.LeaderWinRateHigh > minute.LeaderWinRate);
    }

    [Fact]
    public void LeadMinute_LabelsEachBucketWithItsSoulsRange()
    {
        var buckets = new List<LeadTotals>
        {
            new() { LeadBucket = 3, Games = 100, LeaderWins = 70 },
            new() { LeadBucket = 0, Games = 100, LeaderWins = 55 }
        };

        var minute = CurveAnalyticsService.LeadMinute(9, buckets, 1);

        // Sorted by lead, and each bucket covers a thousand souls.
        Assert.Equal([0, 3000], minute.Buckets.Select(a => a.LeadFrom));
        Assert.Equal([1000, 4000], minute.Buckets.Select(a => a.LeadTo));
        Assert.Equal(0.7, minute.Buckets[1].LeaderWinRate, 9);
        Assert.Equal(0.3, minute.Buckets[1].ComebackRate, 9);
    }

    [Fact]
    public void LeadMinute_TheLastBucketHasNoUpperLimit()
    {
        var buckets = new List<LeadTotals> { new() { LeadBucket = TeamLeadDay.MaxLeadBucket, Games = 10, LeaderWins = 10 } };

        var bucket = CurveAnalyticsService.LeadMinute(30, buckets, 1).Buckets.Single();

        Assert.Equal(30_000, bucket.LeadFrom);
        Assert.Null(bucket.LeadTo);
    }

    [Fact]
    public void LeadMinute_LeavesOutThinBucketsButKeepsTheirGamesInTheTotals()
    {
        var buckets = new List<LeadTotals>
        {
            new() { LeadBucket = 0, Games = 100, LeaderWins = 60 },
            new() { LeadBucket = 9, Games = 5, LeaderWins = 5 }
        };

        var minute = CurveAnalyticsService.LeadMinute(12, buckets, minGames: 50);

        Assert.Equal(105, minute.Games);
        Assert.Equal([0], minute.Buckets.Select(a => a.LeadFrom));
        Assert.Equal(65d / 105, minute.LeaderWinRate, 9);
    }

    [Fact]
    public void SampleTimes_AreEveryThreeMinutesForTheFirstFifteenThenEveryFifteen()
    {
        Assert.Equal([3, 6, 9, 12, 15, 30, 45, 60, 75], HeroCurveDay.SampleTimes.Select(a => a / 60));
    }
}
