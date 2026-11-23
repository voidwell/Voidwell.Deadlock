namespace Voidwell.Deadlock.Analytics.Models;

/// <summary>
/// One item of a suggested build, in buying order.
/// </summary>
/// <param name="Order">Position in buying order, from 1.</param>
/// <param name="ItemId">The item.</param>
/// <param name="Name">The item's name.</param>
/// <param name="ImageUrl">The item's image.</param>
/// <param name="Slot">weapon, spirit or vitality.</param>
/// <param name="Tier">Shop tier.</param>
/// <param name="Cost">Price in souls.</param>
/// <param name="BuyTimeMinutes">When players who bought it usually first bought it (the median).</param>
/// <param name="Core">Bought in so many games that it could not be compared; included because nearly everyone buys it.</param>
/// <param name="Pick">The step at which the item was chosen. Core items come first.</param>
/// <param name="CohortGames">Games that had every item chosen before this one, the games it was judged in.</param>
/// <param name="Games">Of those, the games where it was bought.</param>
/// <param name="PickRate">Share of the cohort that bought it.</param>
/// <param name="WinRate">Win rate of the cohort games where it was bought.</param>
/// <param name="GamesWithout">Cohort games that lasted until <paramref name="BuyTimeMinutes"/> but did not buy it.</param>
/// <param name="WinRateWithout">Win rate of the cohort games that lasted until <paramref name="BuyTimeMinutes"/> but did not buy it.</param>
/// <param name="Lift">
/// How much higher the win rate was with the item than without it, in that cohort. Null when no game went without it.
/// </param>
/// <param name="LiftLow">
/// The lower end of the interval of <paramref name="Lift"/>, widened for the number of items compared at the item's
/// step so that all of them are inside their intervals with 95% probability. Items are chosen by this, and only when it
/// is above zero.
/// </param>
/// <param name="StandaloneLift">The item's lift across all of the hero's games, before any other item was chosen.</param>
/// <param name="Synergy">
/// <paramref name="Lift"/> less <paramref name="StandaloneLift"/>: how much better the item does with the items chosen
/// before it than on its own. Null for core items and the first chosen item.
/// </param>
public record ItemBuildStep(
    int Order,
    long ItemId,
    string? Name,
    string? ImageUrl,
    string? Slot,
    int? Tier,
    int? Cost,
    double BuyTimeMinutes,
    bool Core,
    int Pick,
    int CohortGames,
    int Games,
    double PickRate,
    double WinRate,
    int GamesWithout,
    double WinRateWithout,
    double? Lift,
    double? LiftLow,
    double? StandaloneLift,
    double? Synergy);

/// <param name="Query">The period, mode and ranks covered.</param>
/// <param name="Hero">The hero and its overall result.</param>
/// <param name="Games">The hero's games with item data in the query. Players who left the match are not counted.</param>
/// <param name="WinRate">Win rate of <paramref name="Games"/>.</param>
/// <param name="Items">The build, in buying order.</param>
/// <param name="FullBuildGames">Games where every non-core item of the build was bought.</param>
/// <param name="FullBuildWinRate">Win rate of those games, or null when there are none.</param>
/// <param name="FullBuildWinRateLow">Lower end of the 95% interval of <paramref name="FullBuildWinRate"/>.</param>
/// <param name="FullBuildWinRateHigh">Upper end of the 95% interval of <paramref name="FullBuildWinRate"/>.</param>
/// <param name="StopReason">
/// Why the build has no more items: maxItems (it is full), notEnoughGames (too few games had the items so far to compare
/// another) or noBetterItem (no other item raised the win rate by more than chance could explain).
/// </param>
public record HeroItemBuildResponse(
    RangeInfo Query,
    HeroContext Hero,
    int Games,
    double WinRate,
    List<ItemBuildStep> Items,
    int FullBuildGames,
    double? FullBuildWinRate,
    double? FullBuildWinRateLow,
    double? FullBuildWinRateHigh,
    string StopReason);
