using Microsoft.EntityFrameworkCore;
using Voidwell.Deadlock.Data.Repositories.Abstractions;
using Voidwell.Deadlock.Data.Repositories.Models;

namespace Voidwell.Deadlock.Data.Repositories;

public class MatchupAnalyticsRepository : IMatchupAnalyticsRepository
{
    private readonly IDbContextFactory<DeadlockDbContext> _dbContextFactory;

    public MatchupAnalyticsRepository(IDbContextFactory<DeadlockDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<PairTotals>> GetMatchupTotalsAsync(int heroId, AnalyticsFilter filter)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.HeroMatchupDays
            .Where(a => a.HeroId == heroId)
            .InFilter(filter)
            .GroupBy(a => a.EnemyHeroId)
            .Select(g => new PairTotals
            {
                OtherHeroId = g.Key,
                Games = g.Sum(a => (long)a.Games),
                Wins = g.Sum(a => (long)a.Wins)
            })
            .ToListAsync();
    }

    public async Task<List<PairTotals>> GetSynergyTotalsAsync(int heroId, AnalyticsFilter filter)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.HeroSynergyDays
            .Where(a => a.HeroId == heroId)
            .InFilter(filter)
            .GroupBy(a => a.AllyHeroId)
            .Select(g => new PairTotals
            {
                OtherHeroId = g.Key,
                Games = g.Sum(a => (long)a.Games),
                Wins = g.Sum(a => (long)a.Wins)
            })
            .ToListAsync();
    }

    public async Task<List<PairTotals>> GetLaneTotalsAsync(int heroId, AnalyticsFilter filter)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.HeroLaneDays
            .Where(a => a.HeroId == heroId)
            .InFilter(filter)
            .GroupBy(a => a.EnemyHeroId)
            .Select(g => new PairTotals
            {
                OtherHeroId = g.Key,
                Games = g.Sum(a => (long)a.Games),
                Wins = g.Sum(a => (long)a.Wins),
                LeadGames = g.Sum(a => (long)a.LeadGames),
                LeadSum = g.Sum(a => a.LeadSum)
            })
            .ToListAsync();
    }

    public async Task<List<CompositionTotals>> GetCompositionTotalsAsync(AnalyticsFilter filter)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.TeamCompositionDays
            .InFilter(filter)
            .GroupBy(a => new { a.Brawlers, a.Assassins, a.Marksmen, a.Mystics, a.Supports })
            .Select(g => new CompositionTotals
            {
                Brawlers = g.Key.Brawlers,
                Assassins = g.Key.Assassins,
                Marksmen = g.Key.Marksmen,
                Mystics = g.Key.Mystics,
                Supports = g.Key.Supports,
                Games = g.Sum(a => (long)a.Games),
                Wins = g.Sum(a => (long)a.Wins)
            })
            .OrderByDescending(a => a.Games)
            .ToListAsync();
    }
}
