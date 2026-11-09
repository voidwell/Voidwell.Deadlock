using Voidwell.Deadlock.Analytics.Models;
using Voidwell.Deadlock.CollectionStore.Services.Abstractions;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Abstractions;
using Voidwell.Deadlock.Data.Repositories.Models;

namespace Voidwell.Deadlock.Analytics;

public interface ICurveAnalyticsService
{
    /// <summary>
    /// A hero's average stats over the match, for all its players and for those who won and lost. Null when the hero has
    /// no games in the query.
    /// </summary>
    Task<HeroCurvesResponse?> GetHeroCurvesAsync(int heroId, AnalyticsQuery query, long minSamples);

    /// <summary>
    /// Where a hero's souls come from over the match. Null when the hero has no games in the query.
    /// </summary>
    Task<HeroSoulsSourcesResponse?> GetSoulsSourcesAsync(int heroId, AnalyticsQuery query, long minSamples);

    /// <summary>
    /// How often the team ahead on souls goes on to win, at each sample time and for each size of lead.
    /// <paramref name="minute"/> limits it to one sample time.
    /// </summary>
    Task<LeadResponse> GetLeadAsync(AnalyticsQuery query, int? minute, long minGames);
}

public class CurveAnalyticsService : ICurveAnalyticsService
{
    private const int _secondsPerMinute = 60;

    private readonly ICurveAnalyticsRepository _curveRepository;
    private readonly IHeroAnalyticsRepository _heroRepository;
    private readonly IHeroStore _heroStore;

    public CurveAnalyticsService(ICurveAnalyticsRepository curveRepository, IHeroAnalyticsRepository heroRepository, IHeroStore heroStore)
    {
        _curveRepository = curveRepository;
        _heroRepository = heroRepository;
        _heroStore = heroStore;
    }

    public async Task<HeroCurvesResponse?> GetHeroCurvesAsync(int heroId, AnalyticsQuery query, long minSamples)
    {
        var filter = query.ToFilter();
        var hero = await HeroContexts.LoadAsync(_heroRepository, _heroStore, heroId, filter);
        if (hero == null)
        {
            return null;
        }

        var totals = await _curveRepository.GetHeroCurveTotalsAsync(heroId, filter);

        var points = totals
            .GroupBy(a => a.TimeStampS)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var winners = g.FirstOrDefault(a => a.Won);
                var losers = g.FirstOrDefault(a => !a.Won);

                return new CurvePoint(
                    g.Key / _secondsPerMinute,
                    Stats(Merge(g), minSamples),
                    Stats(winners, minSamples),
                    Stats(losers, minSamples));
            })
            .Where(a => a.All != null || a.Winners != null || a.Losers != null)
            .ToList();

        return new HeroCurvesResponse(Describe(query), hero, points);
    }

    public async Task<HeroSoulsSourcesResponse?> GetSoulsSourcesAsync(int heroId, AnalyticsQuery query, long minSamples)
    {
        var filter = query.ToFilter();
        var hero = await HeroContexts.LoadAsync(_heroRepository, _heroStore, heroId, filter);
        if (hero == null)
        {
            return null;
        }

        var totals = await _curveRepository.GetHeroCurveTotalsAsync(heroId, filter);

        var points = totals
            .GroupBy(a => a.TimeStampS)
            .OrderBy(g => g.Key)
            .Select(g => (Minute: g.Key / _secondsPerMinute, Totals: Merge(g)))
            .Where(a => a.Totals.Samples >= minSamples)
            .Select(a => SoulsSources(a.Minute, a.Totals))
            .ToList();

        return new HeroSoulsSourcesResponse(Describe(query), hero, points);
    }

    public async Task<LeadResponse> GetLeadAsync(AnalyticsQuery query, int? minute, long minGames)
    {
        int? timeStamp = null;
        if (minute != null)
        {
            timeStamp = HeroCurveDay.SampleTimes.Cast<int?>().FirstOrDefault(a => a / _secondsPerMinute == minute);
            if (timeStamp == null)
            {
                var valid = string.Join(", ", HeroCurveDay.SampleTimes.Select(a => a / _secondsPerMinute));
                throw new AnalyticsRequestException($"minute must be one of the sample times: {valid}.");
            }
        }

        var totals = await _curveRepository.GetLeadTotalsAsync(query.ToFilter());

        var minutes = totals
            .Where(a => timeStamp == null || a.TimeStampS == timeStamp)
            .GroupBy(a => a.TimeStampS)
            .OrderBy(g => g.Key)
            .Select(g => LeadMinute(g.Key / _secondsPerMinute, g.ToList(), minGames))
            .ToList();

        return new LeadResponse(Describe(query), minutes);
    }

    /// <summary>
    /// Builds a minute of the lead response from its lead buckets. Buckets with fewer than <paramref name="minGames"/>
    /// matches are left out of the list, but still count towards the minute's totals.
    /// </summary>
    public static LeadMinutePoint LeadMinute(int minute, List<LeadTotals> buckets, long minGames)
    {
        var games = buckets.Sum(a => a.Games);
        var leaderWins = buckets.Sum(a => a.LeaderWins);
        var (low, high) = Statistics.Wilson(leaderWins, games);

        var points = buckets
            .Where(a => a.Games >= minGames)
            .OrderBy(a => a.LeadBucket)
            .Select(a =>
            {
                var (bucketLow, bucketHigh) = Statistics.Wilson(a.LeaderWins, a.Games);
                var rate = Statistics.Divide(a.LeaderWins, a.Games);
                var isLast = a.LeadBucket >= TeamLeadDay.MaxLeadBucket;

                return new LeadBucketPoint(
                    a.LeadBucket * 1000,
                    isLast ? null : (a.LeadBucket + 1) * 1000,
                    (int)a.Games,
                    (int)a.LeaderWins,
                    rate,
                    bucketLow,
                    bucketHigh,
                    1 - rate);
            })
            .ToList();

        var overall = Statistics.Divide(leaderWins, games);

        return new LeadMinutePoint(minute, (int)games, overall, low, high, 1 - overall, points);
    }

    /// <summary>
    /// Splits a point's net worth into where the souls came from. Whatever the named sources do not explain (assists,
    /// breakable objects, team bonuses and so on) is shown as other.
    /// </summary>
    public static SoulsSourcePoint SoulsSources(int minute, CurveTotals totals)
    {
        var n = totals.Samples;
        var netWorth = Statistics.Divide(totals.NetWorth, n);

        var kills = Statistics.Divide(totals.KillSouls, n);
        var lane = Statistics.Divide(totals.LaneSouls, n);
        var neutral = Statistics.Divide(totals.NeutralSouls, n);
        var boss = Statistics.Divide(totals.BossSouls, n);
        var treasure = Statistics.Divide(totals.TreasureSouls, n);
        var other = netWorth - kills - lane - neutral - boss - treasure;

        SoulsSource Source(string name, double souls) => new(name, souls, Statistics.Divide(souls, netWorth));

        return new SoulsSourcePoint(minute, (int)n, netWorth,
        [
            Source("kills", kills),
            Source("laneCreeps", lane),
            Source("neutrals", neutral),
            Source("bosses", boss),
            Source("treasure", treasure),
            Source("other", other)
        ]);
    }

    /// <summary>
    /// Adds the winners' and losers' sums at one sample time.
    /// </summary>
    public static CurveTotals Merge(IEnumerable<CurveTotals> rows)
    {
        var all = new CurveTotals();

        foreach (var row in rows)
        {
            all.TimeStampS = row.TimeStampS;
            all.Samples += row.Samples;
            all.NetWorth += row.NetWorth;
            all.Level += row.Level;
            all.Kills += row.Kills;
            all.Deaths += row.Deaths;
            all.Assists += row.Assists;
            all.PlayerDamage += row.PlayerDamage;
            all.PlayerDamageTaken += row.PlayerDamageTaken;
            all.PlayerHealing += row.PlayerHealing;
            all.WeaponPower += row.WeaponPower;
            all.TechPower += row.TechPower;
            all.MaxHealth += row.MaxHealth;
            all.KillSouls += row.KillSouls;
            all.LaneSouls += row.LaneSouls;
            all.NeutralSouls += row.NeutralSouls;
            all.BossSouls += row.BossSouls;
            all.TreasureSouls += row.TreasureSouls;
        }

        return all;
    }

    private static CurveStats? Stats(CurveTotals? totals, long minSamples)
    {
        return totals != null && totals.Samples >= minSamples && totals.Samples > 0 ? CurveStats.Create(totals) : null;
    }

    private static RangeInfo Describe(AnalyticsQuery query)
    {
        return new RangeInfo(query.From, query.To, query.Mode.ToString().ToLowerInvariant(), query.MinTier, query.MaxTier);
    }
}
