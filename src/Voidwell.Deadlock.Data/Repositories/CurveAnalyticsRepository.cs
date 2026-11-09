using Microsoft.EntityFrameworkCore;
using Voidwell.Deadlock.Data.Repositories.Abstractions;
using Voidwell.Deadlock.Data.Repositories.Models;

namespace Voidwell.Deadlock.Data.Repositories;

public class CurveAnalyticsRepository : ICurveAnalyticsRepository
{
    private readonly IDbContextFactory<DeadlockDbContext> _dbContextFactory;

    public CurveAnalyticsRepository(IDbContextFactory<DeadlockDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<CurveTotals>> GetHeroCurveTotalsAsync(int heroId, AnalyticsFilter filter)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.HeroCurveDays
            .Where(a => a.HeroId == heroId)
            .InFilter(filter)
            .GroupBy(a => new { a.Won, a.TimeStampS })
            .Select(g => new CurveTotals
            {
                Won = g.Key.Won,
                TimeStampS = g.Key.TimeStampS,
                Samples = g.Sum(a => (long)a.Samples),
                NetWorth = g.Sum(a => a.NetWorth),
                Level = g.Sum(a => a.Level),
                Kills = g.Sum(a => a.Kills),
                Deaths = g.Sum(a => a.Deaths),
                Assists = g.Sum(a => a.Assists),
                PlayerDamage = g.Sum(a => a.PlayerDamage),
                PlayerDamageTaken = g.Sum(a => a.PlayerDamageTaken),
                PlayerHealing = g.Sum(a => a.PlayerHealing),
                WeaponPower = g.Sum(a => a.WeaponPower),
                TechPower = g.Sum(a => a.TechPower),
                MaxHealth = g.Sum(a => a.MaxHealth),
                KillSouls = g.Sum(a => a.KillSouls),
                LaneSouls = g.Sum(a => a.LaneSouls),
                NeutralSouls = g.Sum(a => a.NeutralSouls),
                BossSouls = g.Sum(a => a.BossSouls),
                TreasureSouls = g.Sum(a => a.TreasureSouls)
            })
            .ToListAsync();
    }

    public async Task<List<LeadTotals>> GetLeadTotalsAsync(AnalyticsFilter filter)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.TeamLeadDays
            .InFilter(filter)
            .GroupBy(a => new { a.TimeStampS, a.LeadBucket })
            .Select(g => new LeadTotals
            {
                TimeStampS = g.Key.TimeStampS,
                LeadBucket = g.Key.LeadBucket,
                Games = g.Sum(a => (long)a.Games),
                LeaderWins = g.Sum(a => (long)a.LeaderWins)
            })
            .OrderBy(a => a.TimeStampS)
            .ThenBy(a => a.LeadBucket)
            .ToListAsync();
    }
}
