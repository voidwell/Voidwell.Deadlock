namespace Voidwell.Deadlock.Analytics.Models;

public record ItemStatsResponse(RangeInfo Query, int Matches, List<ItemSummary> Items);

/// <summary>
/// The hero fields the hero-specific item, build and ability responses start with.
/// </summary>
public record HeroContext(int HeroId, string? Name, string? IconUrl, int Games, double WinRate);

public record HeroItemsResponse(RangeInfo Query, HeroContext Hero, List<ItemSummary> Items);

public record BuildShapePoint(
    int WeaponItems,
    int SpiritItems,
    int VitalityItems,
    string Label,
    int Games,
    double Share,
    int Wins,
    double WinRate,
    double WinRateLow,
    double WinRateHigh);

public record HeroBuildShapesResponse(RangeInfo Query, HeroContext Hero, List<BuildShapePoint> Shapes);

public record CommunityBuildPoint(
    long BuildId,
    int Games,
    double Share,
    int Wins,
    double WinRate,
    double WinRateLow,
    double WinRateHigh,
    double WinRateLift);

public record HeroCommunityBuildsResponse(RangeInfo Query, HeroContext Hero, double BuildUseRate, List<CommunityBuildPoint> Builds);

public record AbilitySummary(
    long AbilityId,
    string? Name,
    string? ImageUrl,
    int Games,
    double LearnRate,
    double WinRate,
    double MaxedRate,
    double? MaxedWinRate,
    double MaxFirstRate,
    double? MaxFirstWinRate,
    double AvgFirstPointMinutes);

public record AbilityPickOption(long AbilityId, string? Name, int Games, double Share, double WinRate);

public record AbilityPickPoint(int Pick, List<AbilityPickOption> Options);

public record HeroAbilityOrderResponse(
    RangeInfo Query,
    HeroContext Hero,
    List<AbilitySummary> Abilities,
    List<AbilityPickPoint> Picks,
    List<long> SuggestedOrder);
