namespace Voidwell.Deadlock.Analytics.Models;

// All rates are fractions from 0 to 1, and changes in rates are differences of those fractions.

/// <summary>
/// The days, mode and rank tiers a response covers, as they were resolved from the request.
/// </summary>
public record RangeInfo(DateOnly From, DateOnly To, string Mode, int? MinTier, int? MaxTier);

public record HeroStatsResponse(RangeInfo Query, int Matches, List<HeroSummary> Heroes);

public record TierListEntry(string Tier, HeroSummary Hero);

public record TierListResponse(RangeInfo Query, int Matches, string Method, List<TierListEntry> Heroes);

public record HeroDailyPoint(DateOnly Day, int Games, int Wins, double WinRate, double PickRate);

public record HeroTierPoint(int Tier, string TierName, int Games, int Wins, double WinRate, double WinRateLow, double WinRateHigh, double PickRate);

public record HeroDetailResponse(RangeInfo Query, HeroSummary Hero, List<HeroDailyPoint> Daily, List<HeroTierPoint> ByTier);

public record HeroTrend(
    int HeroId,
    string? Name,
    string? IconUrl,
    int Games,
    int BaselineGames,
    double WinRate,
    double BaselineWinRate,
    double WinRateChange,
    double PickRate,
    double BaselinePickRate,
    double PickRateChange,
    double? BanRate,
    double? BaselineBanRate,
    double? BanRateChange,
    double ZScore,
    bool Significant);

public record HeroTrendsResponse(RangeInfo Query, RangeInfo Baseline, List<HeroTrend> Heroes);
