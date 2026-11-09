namespace Voidwell.Deadlock.Analytics.Models;

public record OverviewTotals(int Matches, double AvgMatchMinutes, double Team0WinRate, double Team0WinRateLow, double Team0WinRateHigh, double BanCoverage);

public record DailyMatchPoint(DateOnly Day, int Matches, double AvgMatchMinutes);

public record ModeShare(string Mode, int Matches, double Share);

/// <summary>
/// Matches that lasted from <see cref="FromMinute"/> up to (not including) <see cref="ToMinute"/> minutes. The last
/// bucket has no upper limit.
/// </summary>
public record DurationBucket(int FromMinute, int? ToMinute, int Matches, double Share);

public record DurationSummary(
    double AvgMinutes,
    double P10Minutes,
    double P25Minutes,
    double P50Minutes,
    double P75Minutes,
    double P90Minutes,
    List<DurationBucket> Histogram);

/// <summary>
/// Matches that started in one UTC hour of the day. <see cref="MatchesPerDay"/> is the average over the days in the query.
/// </summary>
public record HourPoint(int Hour, int Matches, double Share, double MatchesPerDay);

public record OverviewResponse(
    RangeInfo Query,
    OverviewTotals Totals,
    List<ModeShare> Modes,
    List<DailyMatchPoint> Daily,
    DurationSummary Duration,
    List<HourPoint> Hours);

public record RankPoint(int Tier, string TierName, int Matches, double Share);

public record RanksResponse(RangeInfo Query, int Matches, List<RankPoint> Ranks);

/// <summary>
/// How teams did when an objective was destroyed by the end of the match, and when it was still standing.
/// </summary>
public record ObjectivePoint(
    int Objective,
    string Name,
    string Type,
    int? Lane,
    int LostGames,
    double LostRate,
    double WinRateWhenLost,
    double WinRateWhenLostLow,
    double WinRateWhenLostHigh,
    int HeldGames,
    double WinRateWhenHeld,
    double WinRateWhenHeldLow,
    double WinRateWhenHeldHigh);

public record ObjectivesResponse(RangeInfo Query, int Teams, List<ObjectivePoint> Objectives);

/// <summary>
/// Teams with a given number of their own and of enemy players who left. Counts of three or more are grouped together.
/// </summary>
public record LeaverScenario(int OwnLeavers, int EnemyLeavers, string Label, int Games, int Wins, double WinRate, double WinRateLow, double WinRateHigh);

public record LeaversResponse(
    RangeInfo Query,
    int Players,
    int Leavers,
    double LeaverRate,
    double MatchesWithLeaverRate,
    double? WinRateWithLeaver,
    double? WinRateAgainstLeaver,
    List<LeaverScenario> Scenarios);
