using Microsoft.EntityFrameworkCore;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Abstractions;
using Voidwell.Deadlock.Data.Repositories.Models;

namespace Voidwell.Deadlock.Data.Repositories;

public class HeroAnalyticsRepository : IHeroAnalyticsRepository
{
    private readonly IDbContextFactory<DeadlockDbContext> _dbContextFactory;

    public HeroAnalyticsRepository(IDbContextFactory<DeadlockDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<DateOnly?> GetLatestDayAsync()
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.HeroDays.MaxAsync(a => (DateOnly?)a.Day);
    }

    public async Task<MatchTotals> GetMatchTotalsAsync(AnalyticsFilter filter)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var rows = await FilterMatchDays(dbContext, filter)
            .GroupBy(a => 1)
            .Select(g => new MatchTotals
            {
                Matches = g.Sum(a => (long)a.Matches),
                MatchesWithBans = g.Sum(a => (long)a.MatchesWithBans),
                WinsTeam0 = g.Sum(a => (long)a.WinsTeam0),
                DurationSumS = g.Sum(a => a.DurationSumS)
            })
            .ToListAsync();

        return rows.FirstOrDefault() ?? new MatchTotals();
    }

    public async Task<List<MatchTotals>> GetMatchTotalsByDayAsync(AnalyticsFilter filter)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await FilterMatchDays(dbContext, filter)
            .GroupBy(a => a.Day)
            .Select(g => new MatchTotals
            {
                Day = g.Key,
                Matches = g.Sum(a => (long)a.Matches),
                MatchesWithBans = g.Sum(a => (long)a.MatchesWithBans),
                WinsTeam0 = g.Sum(a => (long)a.WinsTeam0),
                DurationSumS = g.Sum(a => a.DurationSumS)
            })
            .OrderBy(a => a.Day)
            .ToListAsync();
    }

    public async Task<List<MatchTotals>> GetMatchTotalsByTierAsync(AnalyticsFilter filter)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await FilterMatchDays(dbContext, filter)
            .GroupBy(a => (int)a.Tier)
            .Select(g => new MatchTotals
            {
                Tier = g.Key,
                Matches = g.Sum(a => (long)a.Matches),
                MatchesWithBans = g.Sum(a => (long)a.MatchesWithBans),
                WinsTeam0 = g.Sum(a => (long)a.WinsTeam0),
                DurationSumS = g.Sum(a => a.DurationSumS)
            })
            .OrderBy(a => a.Tier)
            .ToListAsync();
    }

    public async Task<List<MatchTotals>> GetMatchTotalsByModeAsync(AnalyticsFilter filter)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await FilterMatchDays(dbContext, filter)
            .GroupBy(a => (int)a.Mode)
            .Select(g => new MatchTotals
            {
                Mode = g.Key,
                Matches = g.Sum(a => (long)a.Matches),
                MatchesWithBans = g.Sum(a => (long)a.MatchesWithBans),
                WinsTeam0 = g.Sum(a => (long)a.WinsTeam0),
                DurationSumS = g.Sum(a => a.DurationSumS)
            })
            .OrderBy(a => a.Mode)
            .ToListAsync();
    }

    public async Task<List<HeroTotals>> GetHeroTotalsAsync(AnalyticsFilter filter)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await FilterHeroDays(dbContext, filter)
            .GroupBy(a => a.HeroId)
            .Select(g => new HeroTotals
            {
                HeroId = g.Key,
                Games = g.Sum(a => (long)a.Games),
                Wins = g.Sum(a => (long)a.Wins),
                Kills = g.Sum(a => a.Kills),
                Deaths = g.Sum(a => a.Deaths),
                Assists = g.Sum(a => a.Assists),
                NetWorth = g.Sum(a => a.NetWorth),
                LastHits = g.Sum(a => a.LastHits),
                Denies = g.Sum(a => a.Denies),
                DurationSumS = g.Sum(a => a.DurationSumS),
                StatGames = g.Sum(a => (long)a.StatGames),
                PlayerDamage = g.Sum(a => a.PlayerDamage),
                PlayerDamageTaken = g.Sum(a => a.PlayerDamageTaken),
                BossDamage = g.Sum(a => a.BossDamage),
                PlayerHealing = g.Sum(a => a.PlayerHealing),
                ShotsHit = g.Sum(a => a.ShotsHit),
                ShotsMissed = g.Sum(a => a.ShotsMissed)
            })
            .ToListAsync();
    }

    public async Task<List<HeroTotals>> GetHeroTotalsByDayAsync(int heroId, AnalyticsFilter filter)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await FilterHeroDays(dbContext, filter)
            .Where(a => a.HeroId == heroId)
            .GroupBy(a => a.Day)
            .Select(g => new HeroTotals
            {
                HeroId = heroId,
                Day = g.Key,
                Games = g.Sum(a => (long)a.Games),
                Wins = g.Sum(a => (long)a.Wins),
                Kills = g.Sum(a => a.Kills),
                Deaths = g.Sum(a => a.Deaths),
                Assists = g.Sum(a => a.Assists),
                NetWorth = g.Sum(a => a.NetWorth),
                DurationSumS = g.Sum(a => a.DurationSumS)
            })
            .OrderBy(a => a.Day)
            .ToListAsync();
    }

    public async Task<List<HeroTotals>> GetHeroTotalsByTierAsync(int heroId, AnalyticsFilter filter)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await FilterHeroDays(dbContext, filter)
            .Where(a => a.HeroId == heroId)
            .GroupBy(a => (int)a.Tier)
            .Select(g => new HeroTotals
            {
                HeroId = heroId,
                Tier = g.Key,
                Games = g.Sum(a => (long)a.Games),
                Wins = g.Sum(a => (long)a.Wins),
                Kills = g.Sum(a => a.Kills),
                Deaths = g.Sum(a => a.Deaths),
                Assists = g.Sum(a => a.Assists),
                NetWorth = g.Sum(a => a.NetWorth),
                DurationSumS = g.Sum(a => a.DurationSumS)
            })
            .OrderBy(a => a.Tier)
            .ToListAsync();
    }

    public async Task<List<HeroBanTotals>> GetBanTotalsAsync(AnalyticsFilter filter)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var modes = filter.Modes;
        return await dbContext.HeroBanDays
            .Where(a => a.Day >= filter.From && a.Day <= filter.To && modes.Contains(a.Mode) && a.Tier >= filter.MinTier && a.Tier <= filter.MaxTier)
            .GroupBy(a => a.HeroId)
            .Select(g => new HeroBanTotals { HeroId = g.Key, Bans = g.Sum(a => (long)a.Bans) })
            .ToListAsync();
    }

    private static IQueryable<MatchDay> FilterMatchDays(DeadlockDbContext dbContext, AnalyticsFilter filter)
    {
        var modes = filter.Modes;
        return dbContext.MatchDays
            .Where(a => a.Day >= filter.From && a.Day <= filter.To && modes.Contains(a.Mode) && a.Tier >= filter.MinTier && a.Tier <= filter.MaxTier);
    }

    private static IQueryable<HeroDay> FilterHeroDays(DeadlockDbContext dbContext, AnalyticsFilter filter)
    {
        var modes = filter.Modes;
        return dbContext.HeroDays
            .Where(a => a.Day >= filter.From && a.Day <= filter.To && modes.Contains(a.Mode) && a.Tier >= filter.MinTier && a.Tier <= filter.MaxTier);
    }
}
