using Microsoft.EntityFrameworkCore;
using Voidwell.Deadlock.Data.Extensions;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Abstractions;

namespace Voidwell.Deadlock.Data.Repositories;

public class UpdaterSchedulerRepository : IUpdaterSchedulerRepository
{
    private readonly IDbContextFactory<DeadlockDbContext> _dbContextFactory;

    public UpdaterSchedulerRepository(IDbContextFactory<DeadlockDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<UpdaterScheduler?> GetUpdaterHistoryByServiceNameAsync(string serviceName)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.UpdaterScheduler.SingleOrDefaultAsync(u => u.Id == serviceName);
    }

    public async Task UpsertAsync(UpdaterScheduler entity)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.UpsertAsync(entity);
    }
}
