using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Models;

namespace Voidwell.Deadlock.Analytics.Models;

/// <summary>
/// How often a shop item was bought, and how those players did. A game is a player who bought the item at least once.
/// </summary>
public class ItemSummary
{
    public long ItemId { get; init; }
    public string? Name { get; init; }
    public string? ImageUrl { get; init; }
    public string? SlotType { get; init; }
    public int? Tier { get; init; }
    public int? Cost { get; init; }

    public int Games { get; init; }
    public int Wins { get; init; }
    public double WinRate { get; init; }
    public double WinRateLow { get; init; }
    public double WinRateHigh { get; init; }

    /// <summary>
    /// The share of players (of the hero, or of all players) who bought the item.
    /// </summary>
    public double BuyRate { get; init; }

    /// <summary>
    /// The share of buyers who still had the item at the end rather than selling it.
    /// </summary>
    public double KeptRate { get; init; }

    /// <summary>
    /// The win rate of buyers who kept the item. Null when none did.
    /// </summary>
    public double? KeptWinRate { get; init; }

    public double AvgFirstBuyMinutes { get; init; }

    /// <summary>
    /// The item's win rate minus the baseline it is compared with: the hero's win rate, or 50% across all heroes.
    /// Items bought late only turn up in long games, so read this together with <see cref="Timing"/>.
    /// </summary>
    public double WinRateLift { get; init; }

    /// <summary>
    /// Buyers split by when they first bought the item.
    /// </summary>
    public List<ItemTimingPoint> Timing { get; init; } = [];

    public static ItemSummary Create(ItemTotals totals, Item? item, long playerCount, double baselineWinRate)
    {
        var games = totals.Games;
        var (low, high) = Statistics.Wilson(totals.Wins, games);
        var winRate = Statistics.Divide(totals.Wins, games);

        return new ItemSummary
        {
            ItemId = totals.ItemId,
            Name = item?.Name,
            ImageUrl = item?.ImageUrl,
            SlotType = item?.SlotType,
            Tier = item?.Tier,
            Cost = item?.Cost,
            Games = (int)games,
            Wins = (int)totals.Wins,
            WinRate = winRate,
            WinRateLow = low,
            WinRateHigh = high,
            BuyRate = Statistics.Divide(games, playerCount),
            KeptRate = Statistics.Divide(totals.HeldGames, games),
            KeptWinRate = totals.HeldGames > 0 ? Statistics.Divide(totals.HeldWins, totals.HeldGames) : null,
            AvgFirstBuyMinutes = Statistics.Divide(totals.BuyTimeSumS, games) / 60,
            WinRateLift = winRate - baselineWinRate,
            Timing =
            [
                Point("early", totals.EarlyGames, totals.EarlyWins, games),
                Point("mid", totals.MidGames, totals.MidWins, games),
                Point("late", totals.LateGames, totals.LateWins, games),
                Point("endgame", totals.EndgameGames, totals.EndgameWins, games)
            ]
        };
    }

    private static ItemTimingPoint Point(string bucket, long games, long wins, long totalGames)
    {
        return new ItemTimingPoint(bucket, (int)games, Statistics.Divide(games, totalGames), Statistics.Divide(wins, games));
    }
}

/// <summary>
/// Buyers who first bought an item in one stretch of the match. The buckets are early (under 10 minutes), mid (10 to 20),
/// late (20 to 30) and endgame (30 or more).
/// </summary>
public record ItemTimingPoint(string Bucket, int Games, double Share, double WinRate);
