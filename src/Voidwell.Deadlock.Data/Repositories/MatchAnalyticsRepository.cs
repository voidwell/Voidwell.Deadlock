using Microsoft.EntityFrameworkCore;
using Voidwell.Deadlock.Data.Repositories.Abstractions;
using Voidwell.Deadlock.Data.Repositories.Models;

namespace Voidwell.Deadlock.Data.Repositories;

public class MatchAnalyticsRepository : IMatchAnalyticsRepository
{
    private readonly IDbContextFactory<DeadlockDbContext> _dbContextFactory;

    public MatchAnalyticsRepository(IDbContextFactory<DeadlockDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<DurationTotals>> GetDurationTotalsAsync(AnalyticsFilter filter)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.MatchDurationDays
            .InFilter(filter)
            .GroupBy(a => (int)a.Minute)
            .Select(g => new DurationTotals { Minute = g.Key, Matches = g.Sum(a => (long)a.Matches) })
            .OrderBy(a => a.Minute)
            .ToListAsync();
    }

    public async Task<List<HourTotals>> GetHourTotalsAsync(AnalyticsFilter filter)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.MatchHourDays
            .InFilter(filter)
            .GroupBy(a => (int)a.Hour)
            .Select(g => new HourTotals { Hour = g.Key, Matches = g.Sum(a => (long)a.Matches) })
            .OrderBy(a => a.Hour)
            .ToListAsync();
    }

    public async Task<List<ObjectiveTotals>> GetObjectiveTotalsAsync(AnalyticsFilter filter)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.MatchObjectiveDays
            .InFilter(filter)
            .GroupBy(a => (int)a.Objective)
            .Select(g => new ObjectiveTotals
            {
                Objective = g.Key,
                LostGames = g.Sum(a => (long)a.LostGames),
                LostWins = g.Sum(a => (long)a.LostWins),
                HeldGames = g.Sum(a => (long)a.HeldGames),
                HeldWins = g.Sum(a => (long)a.HeldWins)
            })
            .OrderBy(a => a.Objective)
            .ToListAsync();
    }

    public async Task<List<LeaverTotals>> GetLeaverTotalsAsync(AnalyticsFilter filter)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.MatchLeaverDays
            .InFilter(filter)
            .GroupBy(a => new { a.OwnLeavers, a.EnemyLeavers })
            .Select(g => new LeaverTotals
            {
                OwnLeavers = g.Key.OwnLeavers,
                EnemyLeavers = g.Key.EnemyLeavers,
                Games = g.Sum(a => (long)a.Games),
                Wins = g.Sum(a => (long)a.Wins),
                Players = g.Sum(a => a.Players),
                LeaverPlayers = g.Sum(a => a.LeaverPlayers)
            })
            .OrderBy(a => a.OwnLeavers)
            .ThenBy(a => a.EnemyLeavers)
            .ToListAsync();
    }
}
