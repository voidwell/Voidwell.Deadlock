using Voidwell.Deadlock.Analytics.Models;
using Voidwell.Deadlock.CollectionStore.Services.Abstractions;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Abstractions;
using Voidwell.Deadlock.Data.Repositories.Models;

namespace Voidwell.Deadlock.Analytics;

public interface IMatchAnalyticsService
{
    /// <summary>
    /// How many matches were played, how long they lasted, when they started, and how the two teams did.
    /// </summary>
    Task<OverviewResponse> GetOverviewAsync(AnalyticsQuery query);

    /// <summary>
    /// How matches are spread across rank tiers.
    /// </summary>
    Task<RanksResponse> GetRanksAsync(AnalyticsQuery query);

    /// <summary>
    /// How teams did when each objective was destroyed or left standing. Objectives that were never destroyed are left out.
    /// </summary>
    Task<ObjectivesResponse> GetObjectivesAsync(AnalyticsQuery query, long minGames);

    /// <summary>
    /// How often players leave a match, and what it does to their team's chances.
    /// </summary>
    Task<LeaversResponse> GetLeaversAsync(AnalyticsQuery query);
}

public class MatchAnalyticsService : IMatchAnalyticsService
{
    private const int _histogramBucketMinutes = 5;

    private readonly IMatchAnalyticsRepository _matchRepository;
    private readonly IHeroAnalyticsRepository _heroRepository;
    private readonly IRankStore _rankStore;

    public MatchAnalyticsService(IMatchAnalyticsRepository matchRepository, IHeroAnalyticsRepository heroRepository, IRankStore rankStore)
    {
        _matchRepository = matchRepository;
        _heroRepository = heroRepository;
        _rankStore = rankStore;
    }

    public async Task<OverviewResponse> GetOverviewAsync(AnalyticsQuery query)
    {
        var filter = query.ToFilter();

        var totals = await _heroRepository.GetMatchTotalsAsync(filter);
        var modes = await _heroRepository.GetMatchTotalsByModeAsync(filter);
        var days = await _heroRepository.GetMatchTotalsByDayAsync(filter);
        var durations = await _matchRepository.GetDurationTotalsAsync(filter);
        var hours = await _matchRepository.GetHourTotalsAsync(filter);

        var (low, high) = Statistics.Wilson(totals.WinsTeam0, totals.Matches);

        var overview = new OverviewTotals(
            (int)totals.Matches,
            AverageMinutes(totals),
            Statistics.Divide(totals.WinsTeam0, totals.Matches),
            low,
            high,
            Statistics.Divide(totals.MatchesWithBans, totals.Matches));

        var modeShares = modes
            .Select(a => new ModeShare(ModeName(a.Mode), (int)a.Matches, Statistics.Divide(a.Matches, totals.Matches)))
            .ToList();

        var daily = days
            .Select(a => new DailyMatchPoint(a.Day!.Value, (int)a.Matches, AverageMinutes(a)))
            .ToList();

        return new OverviewResponse(Describe(query), overview, modeShares, daily, BuildDurationSummary(durations, AverageMinutes(totals)), HourPoints(hours, query.DayCount));
    }

    public async Task<RanksResponse> GetRanksAsync(AnalyticsQuery query)
    {
        var filter = query.ToFilter();

        var tiers = await _heroRepository.GetMatchTotalsByTierAsync(filter);
        var names = (await _rankStore.GetAllRanks() ?? []).Where(a => a.Name != null).ToDictionary(a => a.Tier, a => a.Name!);
        var allMatches = tiers.Sum(a => a.Matches);

        var ranks = tiers
            .Select(a =>
            {
                var tier = a.Tier!.Value;
                var name = tier == MatchDay.UnrankedTier ? "Unranked" : names.GetValueOrDefault(tier, $"Tier {tier}");

                return new RankPoint(tier, name, (int)a.Matches, Statistics.Divide(a.Matches, allMatches));
            })
            .ToList();

        return new RanksResponse(Describe(query), (int)allMatches, ranks);
    }

    public async Task<ObjectivesResponse> GetObjectivesAsync(AnalyticsQuery query, long minGames)
    {
        var totals = await _matchRepository.GetObjectiveTotalsAsync(query.ToFilter());

        // Each match counts once for each team, so a team's chance of losing an objective is out of twice the matches.
        var teams = totals.Count == 0 ? 0 : totals.Max(a => a.LostGames + a.HeldGames);

        var objectives = totals
            .Where(a => a.LostGames > 0 && a.LostGames >= minGames)
            .Select(a => ToObjectivePoint(a))
            .ToList();

        return new ObjectivesResponse(Describe(query), (int)teams, objectives);
    }

    public async Task<LeaversResponse> GetLeaversAsync(AnalyticsQuery query)
    {
        var totals = await _matchRepository.GetLeaverTotalsAsync(query.ToFilter());
        return Leavers(Describe(query), totals);
    }

    public static ObjectivePoint ToObjectivePoint(ObjectiveTotals totals)
    {
        var (name, type, lane) = ObjectiveInfo.Describe(totals.Objective);
        var teams = totals.LostGames + totals.HeldGames;
        var (lostLow, lostHigh) = Statistics.Wilson(totals.LostWins, totals.LostGames);
        var (heldLow, heldHigh) = Statistics.Wilson(totals.HeldWins, totals.HeldGames);

        return new ObjectivePoint(
            totals.Objective,
            name,
            type,
            lane,
            (int)totals.LostGames,
            Statistics.Divide(totals.LostGames, teams),
            Statistics.Divide(totals.LostWins, totals.LostGames),
            lostLow,
            lostHigh,
            (int)totals.HeldGames,
            Statistics.Divide(totals.HeldWins, totals.HeldGames),
            heldLow,
            heldHigh);
    }

    public static LeaversResponse Leavers(RangeInfo query, List<LeaverTotals> totals)
    {
        var players = totals.Sum(a => a.Players);
        var leavers = totals.Sum(a => a.LeaverPlayers);
        var teamGames = totals.Sum(a => a.Games);

        // Both teams of a match have a row, with the leaver counts the other way round, so the share of rows with any
        // leaver is the share of matches with one.
        var withLeaver = totals.Where(a => a.OwnLeavers > 0 || a.EnemyLeavers > 0).Sum(a => a.Games);

        var own = totals.Where(a => a.OwnLeavers > 0 && a.EnemyLeavers == 0).ToList();
        var enemy = totals.Where(a => a.OwnLeavers == 0 && a.EnemyLeavers > 0).ToList();

        var scenarios = totals
            .Select(a =>
            {
                var (low, high) = Statistics.Wilson(a.Wins, a.Games);
                return new LeaverScenario(a.OwnLeavers, a.EnemyLeavers, LeaverLabel(a.OwnLeavers, a.EnemyLeavers), (int)a.Games, (int)a.Wins, Statistics.Divide(a.Wins, a.Games), low, high);
            })
            .ToList();

        return new LeaversResponse(
            query,
            (int)players,
            (int)leavers,
            Statistics.Divide(leavers, players),
            Statistics.Divide(withLeaver, teamGames),
            WinRate(own),
            WinRate(enemy),
            scenarios);
    }

    public static string LeaverLabel(int own, int enemy)
    {
        static string Count(int n) => n >= MatchLeaverDay.MaxLeavers ? $"{n}+" : n.ToString();

        return own == 0 && enemy == 0
            ? "no leavers"
            : $"{Count(own)} from your team, {Count(enemy)} from the enemy team";
    }

    /// <summary>
    /// The value below which the given fraction of matches fall, from a count of matches for each whole minute of length.
    /// The position inside a minute is interpolated.
    /// </summary>
    public static double Percentile(IReadOnlyList<DurationTotals> histogram, double fraction)
    {
        var total = histogram.Sum(a => a.Matches);
        if (total == 0)
        {
            return 0;
        }

        var target = fraction * total;
        long cumulative = 0;

        foreach (var bucket in histogram.OrderBy(a => a.Minute))
        {
            if (cumulative + bucket.Matches >= target)
            {
                return bucket.Minute + Statistics.Divide(target - cumulative, bucket.Matches);
            }

            cumulative += bucket.Matches;
        }

        return histogram.Max(a => a.Minute);
    }

    private static double? WinRate(List<LeaverTotals> rows)
    {
        var games = rows.Sum(a => a.Games);
        return games == 0 ? null : (double)rows.Sum(a => a.Wins) / games;
    }

    private static double AverageMinutes(MatchTotals totals)
    {
        return Statistics.Divide(totals.DurationSumS, totals.Matches) / 60;
    }

    private static string ModeName(int? mode)
    {
        return mode == (int)MatchMode.Ranked ? "ranked" : "unranked";
    }

    private static RangeInfo Describe(AnalyticsQuery query)
    {
        return new RangeInfo(query.From, query.To, query.Mode.ToString().ToLowerInvariant(), query.MinTier, query.MaxTier);
    }

    private static DurationSummary BuildDurationSummary(List<DurationTotals> durations, double averageMinutes)
    {
        var total = durations.Sum(a => a.Matches);

        var histogram = durations
            .GroupBy(a => a.Minute / _histogramBucketMinutes * _histogramBucketMinutes)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var matches = g.Sum(a => a.Matches);
                var isLast = g.Key >= MatchDurationDay.MaxMinute;

                return new DurationBucket(g.Key, isLast ? null : g.Key + _histogramBucketMinutes, (int)matches, Statistics.Divide(matches, total));
            })
            .ToList();

        return new DurationSummary(
            averageMinutes,
            Percentile(durations, 0.10),
            Percentile(durations, 0.25),
            Percentile(durations, 0.50),
            Percentile(durations, 0.75),
            Percentile(durations, 0.90),
            histogram);
    }

    private static List<HourPoint> HourPoints(List<HourTotals> hours, int dayCount)
    {
        var total = hours.Sum(a => a.Matches);
        var byHour = hours.ToDictionary(a => a.Hour, a => a.Matches);

        return Enumerable.Range(0, 24)
            .Select(hour =>
            {
                var matches = byHour.GetValueOrDefault(hour);
                return new HourPoint(hour, (int)matches, Statistics.Divide(matches, total), Statistics.Divide(matches, dayCount));
            })
            .ToList();
    }
}
