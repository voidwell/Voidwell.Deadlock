using Microsoft.EntityFrameworkCore;
using Voidwell.Deadlock.Data.Extensions;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Abstractions;

namespace Voidwell.Deadlock.Data.Repositories;

public class PatchRepository : IPatchRepository
{
    private readonly IDbContextFactory<DeadlockDbContext> _dbContextFactory;

    public PatchRepository(IDbContextFactory<DeadlockDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IEnumerable<Patch>> GetAllPatchesAsync()
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Patches.OrderBy(a => a.PublishedAt).ToListAsync();
    }

    public async Task UpsertRangeAsync(IEnumerable<Patch> entities)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.UpsertAsync(entities);
    }
}
