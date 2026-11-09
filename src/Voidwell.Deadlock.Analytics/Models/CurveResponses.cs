using Voidwell.Deadlock.Data.Repositories.Models;

namespace Voidwell.Deadlock.Analytics.Models;

/// <summary>
/// The average of one hero's players' stats at one point in the match. The stats are cumulative, so each value is the total
/// so far.
/// </summary>
public class CurveStats
{
    public int Samples { get; init; }
    public double NetWorth { get; init; }
    public double Level { get; init; }
    public double Kills { get; init; }
    public double Deaths { get; init; }
    public double Assists { get; init; }
    public double PlayerDamage { get; init; }
    public double PlayerDamageTaken { get; init; }
    public double PlayerHealing { get; init; }
    public double WeaponPower { get; init; }
    public double TechPower { get; init; }
    public double MaxHealth { get; init; }

    public static CurveStats Create(CurveTotals totals)
    {
        var n = totals.Samples;

        return new CurveStats
        {
            Samples = (int)n,
            NetWorth = Statistics.Divide(totals.NetWorth, n),
            Level = Statistics.Divide(totals.Level, n),
            Kills = Statistics.Divide(totals.Kills, n),
            Deaths = Statistics.Divide(totals.Deaths, n),
            Assists = Statistics.Divide(totals.Assists, n),
            PlayerDamage = Statistics.Divide(totals.PlayerDamage, n),
            PlayerDamageTaken = Statistics.Divide(totals.PlayerDamageTaken, n),
            PlayerHealing = Statistics.Divide(totals.PlayerHealing, n),
            WeaponPower = Statistics.Divide(totals.WeaponPower, n),
            TechPower = Statistics.Divide(totals.TechPower, n),
            MaxHealth = Statistics.Divide(totals.MaxHealth, n)
        };
    }
}

/// <summary>
/// One sample time of a hero's curves. Stats are left out (null) when too few players have a sample to average.
/// </summary>
public record CurvePoint(int Minute, CurveStats? All, CurveStats? Winners, CurveStats? Losers);

public record HeroCurvesResponse(RangeInfo Query, HeroContext Hero, List<CurvePoint> Points);

/// <summary>
/// Souls from one source: the average per player and its share of the player's net worth.
/// </summary>
public record SoulsSource(string Source, double Souls, double Share);

public record SoulsSourcePoint(int Minute, int Samples, double NetWorth, List<SoulsSource> Sources);

public record HeroSoulsSourcesResponse(RangeInfo Query, HeroContext Hero, List<SoulsSourcePoint> Points);

/// <summary>
/// Matches whose leading team was ahead by <see cref="LeadFrom"/> up to (not including) <see cref="LeadTo"/> souls.
/// <see cref="LeadTo"/> is null for the last bucket, which has no upper limit.
/// </summary>
public record LeadBucketPoint(int LeadFrom, int? LeadTo, int Games, int LeaderWins, double LeaderWinRate, double LeaderWinRateLow, double LeaderWinRateHigh, double ComebackRate);

public record LeadMinutePoint(int Minute, int Games, double LeaderWinRate, double LeaderWinRateLow, double LeaderWinRateHigh, double ComebackRate, List<LeadBucketPoint> Buckets);

public record LeadResponse(RangeInfo Query, List<LeadMinutePoint> Minutes);
