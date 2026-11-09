using Voidwell.Deadlock.Analytics.Models;
using Voidwell.Deadlock.CollectionStore.Services.Abstractions;
using Voidwell.Deadlock.Data.Repositories.Abstractions;
using Voidwell.Deadlock.Data.Repositories.Models;

namespace Voidwell.Deadlock.Analytics;

public interface IMatchupAnalyticsService
{
    /// <summary>
    /// How a hero does against each enemy hero. Null when the hero has no games in the query.
    /// </summary>
    Task<HeroPairsResponse?> GetCountersAsync(int heroId, AnalyticsQuery query, long minGames);

    /// <summary>
    /// How a hero does with each teammate hero. Null when the hero has no games in the query.
    /// </summary>
    Task<HeroPairsResponse?> GetSynergiesAsync(int heroId, AnalyticsQuery query, long minGames);

    /// <summary>
    /// How a hero does against each hero it faces in its lane, with the souls lead at nine minutes. Null when the hero
    /// has no games in the query.
    /// </summary>
    Task<HeroPairsResponse?> GetLaneMatchupsAsync(int heroId, AnalyticsQuery query, long minGames);

    Task<CompositionsResponse> GetCompositionsAsync(AnalyticsQuery query, long minGames);
}

public class MatchupAnalyticsService : IMatchupAnalyticsService
{
    private readonly IMatchupAnalyticsRepository _matchupRepository;
    private readonly IHeroAnalyticsRepository _heroRepository;
    private readonly IHeroStore _heroStore;

    public MatchupAnalyticsService(IMatchupAnalyticsRepository matchupRepository, IHeroAnalyticsRepository heroRepository, IHeroStore heroStore)
    {
        _matchupRepository = matchupRepository;
        _heroRepository = heroRepository;
        _heroStore = heroStore;
    }

    public Task<HeroPairsResponse?> GetCountersAsync(int heroId, AnalyticsQuery query, long minGames)
    {
        return GetPairsAsync(heroId, query, minGames, opponent: true, (id, filter) => _matchupRepository.GetMatchupTotalsAsync(id, filter));
    }

    public Task<HeroPairsResponse?> GetSynergiesAsync(int heroId, AnalyticsQuery query, long minGames)
    {
        return GetPairsAsync(heroId, query, minGames, opponent: false, (id, filter) => _matchupRepository.GetSynergyTotalsAsync(id, filter));
    }

    public Task<HeroPairsResponse?> GetLaneMatchupsAsync(int heroId, AnalyticsQuery query, long minGames)
    {
        return GetPairsAsync(heroId, query, minGames, opponent: true, (id, filter) => _matchupRepository.GetLaneTotalsAsync(id, filter));
    }

    public async Task<CompositionsResponse> GetCompositionsAsync(AnalyticsQuery query, long minGames)
    {
        var totals = await _matchupRepository.GetCompositionTotalsAsync(query.ToFilter());
        var allGames = totals.Sum(a => a.Games);

        var compositions = totals
            .Where(a => a.Games >= minGames)
            .Select(a =>
            {
                var (low, high) = Statistics.Wilson(a.Wins, a.Games);

                return new CompositionPoint(
                    a.Brawlers,
                    a.Assassins,
                    a.Marksmen,
                    a.Mystics,
                    a.Supports,
                    CompositionLabel(a),
                    (int)a.Games,
                    Statistics.Divide(a.Games, allGames),
                    (int)a.Wins,
                    Statistics.Divide(a.Wins, a.Games),
                    low,
                    high);
            })
            .ToList();

        return new CompositionsResponse(Describe(query), (int)allGames, compositions);
    }

    public static string CompositionLabel(CompositionTotals composition)
    {
        var parts = new List<string>();
        AddPart(parts, composition.Brawlers, "brawler");
        AddPart(parts, composition.Assassins, "assassin");
        AddPart(parts, composition.Marksmen, "marksman");
        AddPart(parts, composition.Mystics, "mystic");
        AddPart(parts, composition.Supports, "support");

        return parts.Count == 0 ? "none" : string.Join(", ", parts);
    }

    private static void AddPart(List<string> parts, int count, string name)
    {
        if (count > 0)
        {
            parts.Add($"{count} {name}");
        }
    }

    private static RangeInfo Describe(AnalyticsQuery query)
    {
        return new RangeInfo(query.From, query.To, query.Mode.ToString().ToLowerInvariant(), query.MinTier, query.MaxTier);
    }

    private async Task<HeroPairsResponse?> GetPairsAsync(int heroId, AnalyticsQuery query, long minGames, bool opponent,
        Func<int, AnalyticsFilter, Task<List<PairTotals>>> load)
    {
        var filter = query.ToFilter();

        // Every hero's overall win rate, which the pair results are compared against.
        var heroTotals = (await _heroRepository.GetHeroTotalsAsync(filter)).ToDictionary(a => a.HeroId);
        if (!heroTotals.TryGetValue(heroId, out var self) || self.Games == 0)
        {
            return null;
        }

        var heroes = (await _heroStore.GetAllHeroes() ?? []).ToDictionary(a => a.Id);
        var heroWinRate = Statistics.Divide(self.Wins, self.Games);

        var hero = heroes.GetValueOrDefault(heroId);
        var context = new HeroContext(heroId, hero?.Name, hero?.IconUrl, (int)self.Games, heroWinRate);

        var pairs = (await load(heroId, filter))
            .Where(a => a.Games >= minGames)
            .Select(a => PairSummary.Create(
                a,
                heroes.GetValueOrDefault(a.OtherHeroId),
                heroWinRate,
                heroTotals.TryGetValue(a.OtherHeroId, out var other) ? Statistics.Divide(other.Wins, other.Games) : 0.5,
                opponent))
            .OrderByDescending(a => a.Advantage)
            .ToList();

        return new HeroPairsResponse(Describe(query), context, pairs);
    }
}
