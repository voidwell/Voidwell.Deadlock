using Microsoft.EntityFrameworkCore;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Abstractions;
using Voidwell.Deadlock.Data.Repositories.Models;

namespace Voidwell.Deadlock.Data.Repositories;

public class ItemAnalyticsRepository : IItemAnalyticsRepository
{
    private readonly IDbContextFactory<DeadlockDbContext> _dbContextFactory;

    public ItemAnalyticsRepository(IDbContextFactory<DeadlockDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<ItemTotals>> GetItemTotalsAsync(AnalyticsFilter filter)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await SumByItem(dbContext.ItemDays.InFilter(filter)).ToListAsync();
    }

    public async Task<List<ItemTotals>> GetHeroItemTotalsAsync(int heroId, AnalyticsFilter filter)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await SumByItem(dbContext.HeroItemDays.Where(a => a.HeroId == heroId).InFilter(filter)).ToListAsync();
    }

    public async Task<List<BuildShapeTotals>> GetBuildShapeTotalsAsync(int heroId, AnalyticsFilter filter)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.HeroBuildShapeDays.Where(a => a.HeroId == heroId).InFilter(filter)
            .GroupBy(a => new { a.WeaponItems, a.SpiritItems, a.VitalityItems })
            .Select(g => new BuildShapeTotals
            {
                WeaponItems = g.Key.WeaponItems,
                SpiritItems = g.Key.SpiritItems,
                VitalityItems = g.Key.VitalityItems,
                Games = g.Sum(a => (long)a.Games),
                Wins = g.Sum(a => (long)a.Wins)
            })
            .OrderByDescending(a => a.Games)
            .ToListAsync();
    }

    public async Task<List<CommunityBuildTotals>> GetCommunityBuildTotalsAsync(int heroId, AnalyticsFilter filter, long minGames, int limit)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.HeroBuildDays.Where(a => a.HeroId == heroId).InFilter(filter)
            .GroupBy(a => a.BuildId)
            .Select(g => new CommunityBuildTotals
            {
                BuildId = g.Key,
                Games = g.Sum(a => (long)a.Games),
                Wins = g.Sum(a => (long)a.Wins)
            })
            .Where(a => a.Games >= minGames)
            .OrderByDescending(a => a.Games)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<long> GetCommunityBuildGamesAsync(int heroId, AnalyticsFilter filter)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.HeroBuildDays.Where(a => a.HeroId == heroId).InFilter(filter)
            .SumAsync(a => (long)a.Games);
    }

    public async Task<List<AbilityTotals>> GetAbilityTotalsAsync(int heroId, AnalyticsFilter filter)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.HeroAbilityDays.Where(a => a.HeroId == heroId).InFilter(filter)
            .GroupBy(a => a.AbilityId)
            .Select(g => new AbilityTotals
            {
                AbilityId = g.Key,
                Games = g.Sum(a => (long)a.Games),
                Wins = g.Sum(a => (long)a.Wins),
                MaxedGames = g.Sum(a => (long)a.MaxedGames),
                MaxedWins = g.Sum(a => (long)a.MaxedWins),
                MaxFirstGames = g.Sum(a => (long)a.MaxFirstGames),
                MaxFirstWins = g.Sum(a => (long)a.MaxFirstWins),
                UnlockTimeSumS = g.Sum(a => a.UnlockTimeSumS)
            })
            .ToListAsync();
    }

    public async Task<List<AbilityPickTotals>> GetAbilityPickTotalsAsync(int heroId, AnalyticsFilter filter)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.HeroAbilityPickDays.Where(a => a.HeroId == heroId).InFilter(filter)
            .GroupBy(a => new { a.PickIndex, a.AbilityId })
            .Select(g => new AbilityPickTotals
            {
                PickIndex = g.Key.PickIndex,
                AbilityId = g.Key.AbilityId,
                Games = g.Sum(a => (long)a.Games),
                Wins = g.Sum(a => (long)a.Wins)
            })
            .OrderBy(a => a.PickIndex)
            .ToListAsync();
    }

    private static IQueryable<ItemTotals> SumByItem<T>(IQueryable<T> query)
        where T : ItemRollupStats
    {
        return query
            .GroupBy(a => a.ItemId)
            .Select(g => new ItemTotals
            {
                ItemId = g.Key,
                Games = g.Sum(a => (long)a.Games),
                Wins = g.Sum(a => (long)a.Wins),
                HeldGames = g.Sum(a => (long)a.HeldGames),
                HeldWins = g.Sum(a => (long)a.HeldWins),
                BuyTimeSumS = g.Sum(a => a.BuyTimeSumS),
                EarlyGames = g.Sum(a => (long)a.EarlyGames),
                EarlyWins = g.Sum(a => (long)a.EarlyWins),
                MidGames = g.Sum(a => (long)a.MidGames),
                MidWins = g.Sum(a => (long)a.MidWins),
                LateGames = g.Sum(a => (long)a.LateGames),
                LateWins = g.Sum(a => (long)a.LateWins),
                EndgameGames = g.Sum(a => (long)a.EndgameGames),
                EndgameWins = g.Sum(a => (long)a.EndgameWins)
            });
    }
}
