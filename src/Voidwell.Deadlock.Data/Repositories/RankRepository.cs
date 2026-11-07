using Microsoft.EntityFrameworkCore;
using Voidwell.Deadlock.Data.Extensions;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Abstractions;

namespace Voidwell.Deadlock.Data.Repositories;

public class RankRepository : IRankRepository
{
    private readonly IDbContextFactory<DeadlockDbContext> _dbContextFactory;

    public RankRepository(IDbContextFactory<DeadlockDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IEnumerable<Rank>> GetAllRanksAsync()
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Ranks.OrderBy(a => a.Tier).ToListAsync();
    }

    public async Task UpsertRangeAsync(IEnumerable<Rank> entities)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.UpsertAsync(entities);
    }
}
