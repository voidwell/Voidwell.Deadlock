using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Models;

namespace Voidwell.Deadlock.Analytics.Models;

/// <summary>
/// A hero's results over a query. Per-minute rates use the length of the matches played.
/// </summary>
public class HeroSummary
{
    public int HeroId { get; init; }
    public string? Name { get; init; }
    public string? ImageUrl { get; init; }
    public string? IconUrl { get; init; }

    // Counts are ints so they are written as JSON numbers; the API writes every long as a string.
    public int Games { get; init; }
    public int Wins { get; init; }
    public double WinRate { get; init; }
    public double WinRateLow { get; init; }
    public double WinRateHigh { get; init; }

    /// <summary>
    /// The hero's share of all player slots (twelve per match), so the rates of all heroes add up to one.
    /// </summary>
    public double PickRate { get; init; }

    /// <summary>
    /// Bans per match that recorded bans. Null when no match in the query recorded any.
    /// </summary>
    public double? BanRate { get; init; }

    public double AvgKills { get; init; }
    public double AvgDeaths { get; init; }
    public double AvgAssists { get; init; }
    public double Kda { get; init; }
    public double AvgMatchMinutes { get; init; }
    public double SoulsPerMinute { get; init; }
    public double LastHitsPerMinute { get; init; }

    // These come from each player's final stat sample, so they are null when none of the games had one.
    public double? DamagePerMinute { get; init; }
    public double? DamageTakenPerMinute { get; init; }
    public double? BossDamagePerMinute { get; init; }
    public double? HealingPerMinute { get; init; }
    public double? Accuracy { get; init; }

    public const int SlotsPerMatch = 12;

    public static HeroSummary Create(HeroTotals totals, long matches, long matchesWithBans, long bans, Hero? hero)
    {
        var games = totals.Games;
        var (low, high) = Statistics.Wilson(totals.Wins, games);
        var minutes = totals.DurationSumS / 60.0;

        // Stat totals only cover the games that had a final sample, so scale the minutes to match.
        var statMinutes = games == 0 ? 0 : minutes * totals.StatGames / games;
        var hasStats = totals.StatGames > 0 && statMinutes > 0;
        var shots = totals.ShotsHit + totals.ShotsMissed;

        return new HeroSummary
        {
            HeroId = totals.HeroId,
            Name = hero?.Name,
            ImageUrl = hero?.ImageUrl,
            IconUrl = hero?.IconUrl,
            Games = (int)games,
            Wins = (int)totals.Wins,
            WinRate = Statistics.Divide(totals.Wins, games),
            WinRateLow = low,
            WinRateHigh = high,
            PickRate = Statistics.Divide(games, matches * SlotsPerMatch),
            BanRate = matchesWithBans > 0 ? Statistics.Divide(bans, matchesWithBans) : null,
            AvgKills = Statistics.Divide(totals.Kills, games),
            AvgDeaths = Statistics.Divide(totals.Deaths, games),
            AvgAssists = Statistics.Divide(totals.Assists, games),
            Kda = Statistics.Divide(totals.Kills + totals.Assists, Math.Max(1, totals.Deaths)),
            AvgMatchMinutes = Statistics.Divide(minutes, games),
            SoulsPerMinute = Statistics.Divide(totals.NetWorth, minutes),
            LastHitsPerMinute = Statistics.Divide(totals.LastHits, minutes),
            DamagePerMinute = hasStats ? totals.PlayerDamage / statMinutes : null,
            DamageTakenPerMinute = hasStats ? totals.PlayerDamageTaken / statMinutes : null,
            BossDamagePerMinute = hasStats ? totals.BossDamage / statMinutes : null,
            HealingPerMinute = hasStats ? totals.PlayerHealing / statMinutes : null,
            Accuracy = shots > 0 ? (double)totals.ShotsHit / shots : null
        };
    }
}
