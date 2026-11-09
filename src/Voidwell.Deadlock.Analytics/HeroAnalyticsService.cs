using Voidwell.Deadlock.Analytics.Models;
using Voidwell.Deadlock.CollectionStore.Services.Abstractions;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Abstractions;
using Voidwell.Deadlock.Data.Repositories.Models;

namespace Voidwell.Deadlock.Analytics;

public interface IHeroAnalyticsService
{
    Task<HeroStatsResponse> GetHeroStatsAsync(AnalyticsQuery query, long minGames);

    Task<TierListResponse> GetTierListAsync(AnalyticsQuery query, long minGames);

    /// <summary>
    /// One hero's summary, daily results and results by rank tier. Null when the hero has no games in the query.
    /// </summary>
    Task<HeroDetailResponse?> GetHeroDetailAsync(int heroId, AnalyticsQuery query);

    Task<HeroTrendsResponse> GetTrendsAsync(AnalyticsQuery query, AnalyticsQuery baseline, long minGames);
}

public class HeroAnalyticsService : IHeroAnalyticsService
{
    private const string _unrankedName = "Unranked";

    private readonly IHeroAnalyticsRepository _repository;
    private readonly IHeroStore _heroStore;
    private readonly IRankStore _rankStore;

    public HeroAnalyticsService(IHeroAnalyticsRepository repository, IHeroStore heroStore, IRankStore rankStore)
    {
        _repository = repository;
        _heroStore = heroStore;
        _rankStore = rankStore;
    }

    public async Task<HeroStatsResponse> GetHeroStatsAsync(AnalyticsQuery query, long minGames)
    {
        var data = await LoadAsync(query);

        var heroes = data.Totals
            .Where(a => a.Games >= minGames)
            .Select(data.Summarize)
            .OrderByDescending(a => a.Games)
            .ToList();

        return new HeroStatsResponse(Describe(query), (int)data.Matches.Matches, heroes);
    }

    public async Task<TierListResponse> GetTierListAsync(AnalyticsQuery query, long minGames)
    {
        var data = await LoadAsync(query);

        var heroes = data.Totals
            .Where(a => a.Games >= minGames)
            .Select(data.Summarize)
            .OrderByDescending(a => a.WinRate)
            .ThenByDescending(a => a.Games)
            .Select(a => new TierListEntry(TierRating.FromWinRate(a.WinRate), a))
            .ToList();

        return new TierListResponse(Describe(query), (int)data.Matches.Matches, TierRating.Method, heroes);
    }

    public async Task<HeroDetailResponse?> GetHeroDetailAsync(int heroId, AnalyticsQuery query)
    {
        var data = await LoadAsync(query);

        var totals = data.Totals.FirstOrDefault(a => a.HeroId == heroId);
        if (totals == null || totals.Games == 0)
        {
            return null;
        }

        var filter = query.ToFilter();
        var heroDays = await _repository.GetHeroTotalsByDayAsync(heroId, filter);
        var matchDays = (await _repository.GetMatchTotalsByDayAsync(filter)).ToDictionary(a => a.Day!.Value);
        var heroTiers = await _repository.GetHeroTotalsByTierAsync(heroId, filter);
        var matchTiers = (await _repository.GetMatchTotalsByTierAsync(filter)).ToDictionary(a => a.Tier!.Value);
        var rankNames = await GetRankNamesAsync();

        var daily = heroDays
            .Select(a => new HeroDailyPoint(
                a.Day!.Value,
                (int)a.Games,
                (int)a.Wins,
                Statistics.Divide(a.Wins, a.Games),
                Statistics.Divide(a.Games, MatchesOf(matchDays, a.Day.Value) * HeroSummary.SlotsPerMatch)))
            .ToList();

        var byTier = heroTiers
            .Select(a =>
            {
                var (low, high) = Statistics.Wilson(a.Wins, a.Games);
                var tier = a.Tier!.Value;
                var tierName = tier == MatchDay.UnrankedTier ? _unrankedName : rankNames.GetValueOrDefault(tier, $"Tier {tier}");
                var matches = matchTiers.TryGetValue(tier, out var m) ? m.Matches : 0;

                return new HeroTierPoint(tier, tierName, (int)a.Games, (int)a.Wins, Statistics.Divide(a.Wins, a.Games), low, high,
                    Statistics.Divide(a.Games, matches * HeroSummary.SlotsPerMatch));
            })
            .ToList();

        return new HeroDetailResponse(Describe(query), data.Summarize(totals), daily, byTier);
    }

    public async Task<HeroTrendsResponse> GetTrendsAsync(AnalyticsQuery query, AnalyticsQuery baseline, long minGames)
    {
        var current = await LoadAsync(query);
        var previous = await LoadAsync(baseline);
        var previousByHero = previous.Totals.ToDictionary(a => a.HeroId);

        var trends = new List<HeroTrend>();
        foreach (var totals in current.Totals.Where(a => a.Games >= minGames))
        {
            if (!previousByHero.TryGetValue(totals.HeroId, out var before) || before.Games < minGames)
            {
                continue;
            }

            var now = current.Summarize(totals);
            var then = previous.Summarize(before);
            var z = Statistics.TwoProportionZ(totals.Wins, totals.Games, before.Wins, before.Games);

            trends.Add(new HeroTrend(
                totals.HeroId,
                now.Name,
                now.IconUrl,
                (int)totals.Games,
                (int)before.Games,
                now.WinRate,
                then.WinRate,
                now.WinRate - then.WinRate,
                now.PickRate,
                then.PickRate,
                now.PickRate - then.PickRate,
                now.BanRate,
                then.BanRate,
                now.BanRate - then.BanRate,
                z,
                Math.Abs(z) >= 1.96));
        }

        trends = trends.OrderByDescending(a => Math.Abs(a.WinRateChange)).ToList();

        return new HeroTrendsResponse(Describe(query), Describe(baseline), trends);
    }

    private static RangeInfo Describe(AnalyticsQuery query)
    {
        return new RangeInfo(query.From, query.To, query.Mode.ToString().ToLowerInvariant(), query.MinTier, query.MaxTier);
    }

    private static long MatchesOf(Dictionary<DateOnly, MatchTotals> matchDays, DateOnly day)
    {
        return matchDays.TryGetValue(day, out var totals) ? totals.Matches : 0;
    }

    private async Task<Dictionary<int, string>> GetRankNamesAsync()
    {
        var ranks = await _rankStore.GetAllRanks() ?? [];
        return ranks.Where(a => a.Name != null).ToDictionary(a => a.Tier, a => a.Name!);
    }

    private async Task<HeroDataSet> LoadAsync(AnalyticsQuery query)
    {
        var filter = query.ToFilter();

        var matches = await _repository.GetMatchTotalsAsync(filter);
        var totals = await _repository.GetHeroTotalsAsync(filter);
        var bans = (await _repository.GetBanTotalsAsync(filter)).ToDictionary(a => a.HeroId, a => a.Bans);
        var heroes = (await _heroStore.GetAllHeroes() ?? []).ToDictionary(a => a.Id);

        return new HeroDataSet(matches, totals, bans, heroes);
    }

    /// <summary>
    /// The rollup data a query needs, plus what is needed to turn a hero's totals into a summary.
    /// </summary>
    private sealed class HeroDataSet
    {
        private readonly Dictionary<int, long> _bans;
        private readonly Dictionary<int, Hero> _heroes;

        public HeroDataSet(MatchTotals matches, List<HeroTotals> totals, Dictionary<int, long> bans, Dictionary<int, Hero> heroes)
        {
            Matches = matches;
            Totals = totals;
            _bans = bans;
            _heroes = heroes;
        }

        public MatchTotals Matches { get; }
        public List<HeroTotals> Totals { get; }

        public HeroSummary Summarize(HeroTotals totals)
        {
            return HeroSummary.Create(
                totals,
                Matches.Matches,
                Matches.MatchesWithBans,
                _bans.GetValueOrDefault(totals.HeroId),
                _heroes.GetValueOrDefault(totals.HeroId));
        }
    }
}
