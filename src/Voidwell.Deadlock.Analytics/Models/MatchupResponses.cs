using Voidwell.Deadlock.Data.Repositories.Models;

namespace Voidwell.Deadlock.Analytics.Models;

/// <summary>
/// A hero's results against or with one other hero. <see cref="ExpectedWinRate"/> is what the two heroes' overall win
/// rates alone would predict, so <see cref="Advantage"/> shows how much the pairing itself adds or costs.
/// </summary>
public class PairSummary
{
    public int HeroId { get; init; }
    public string? Name { get; init; }
    public string? IconUrl { get; init; }

    public int Games { get; init; }
    public int Wins { get; init; }
    public double WinRate { get; init; }
    public double WinRateLow { get; init; }
    public double WinRateHigh { get; init; }
    public double ExpectedWinRate { get; init; }
    public double Advantage { get; init; }

    /// <summary>
    /// Lane matchups only: the average souls lead of the hero's player over the lane opponent at nine minutes. Null for
    /// other pairs, and when no game had both samples.
    /// </summary>
    public double? AvgLeadAtNineMinutes { get; init; }

    /// <summary>
    /// For a matchup against an enemy hero, the expected win rate is the average of this hero's win rate and the enemy's
    /// loss rate. For a teammate, it is the average of the two heroes' win rates.
    /// </summary>
    public static double Expected(double heroWinRate, double otherWinRate, bool opponent)
    {
        return opponent ? (heroWinRate + (1 - otherWinRate)) / 2 : (heroWinRate + otherWinRate) / 2;
    }

    public static PairSummary Create(PairTotals totals, Data.Models.Hero? other, double heroWinRate, double otherWinRate, bool opponent)
    {
        var (low, high) = Statistics.Wilson(totals.Wins, totals.Games);
        var winRate = Statistics.Divide(totals.Wins, totals.Games);
        var expected = Expected(heroWinRate, otherWinRate, opponent);

        return new PairSummary
        {
            HeroId = totals.OtherHeroId,
            Name = other?.Name,
            IconUrl = other?.IconUrl,
            Games = (int)totals.Games,
            Wins = (int)totals.Wins,
            WinRate = winRate,
            WinRateLow = low,
            WinRateHigh = high,
            ExpectedWinRate = expected,
            Advantage = winRate - expected,
            AvgLeadAtNineMinutes = totals.LeadGames > 0 ? (double)totals.LeadSum / totals.LeadGames : null
        };
    }
}

public record HeroPairsResponse(RangeInfo Query, HeroContext Hero, List<PairSummary> Heroes);

public record CompositionPoint(
    int Brawlers,
    int Assassins,
    int Marksmen,
    int Mystics,
    int Supports,
    string Label,
    int Games,
    double Share,
    int Wins,
    double WinRate,
    double WinRateLow,
    double WinRateHigh);

public record CompositionsResponse(RangeInfo Query, int Teams, List<CompositionPoint> Compositions);
