using Microsoft.EntityFrameworkCore;
using Voidwell.Deadlock.Data.Extensions;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Abstractions;

namespace Voidwell.Deadlock.Data.Repositories;

public class HeroRepository : IHeroRepository
{
    private readonly IDbContextFactory<DeadlockDbContext> _dbContextFactory;

    public HeroRepository(IDbContextFactory<DeadlockDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IEnumerable<Hero>> GetAllHeroesAsync()
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Heroes.OrderBy(a => a.Name).ToListAsync();
    }

    public async Task<Hero?> GetHeroByIdAsync(int id)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Heroes.SingleOrDefaultAsync(a => a.Id == id);
    }

    public async Task UpsertRangeAsync(IEnumerable<Hero> entities)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.UpsertAsync(entities);
    }
}
