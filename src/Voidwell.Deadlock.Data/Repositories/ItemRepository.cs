using Microsoft.EntityFrameworkCore;
using Voidwell.Deadlock.Data.Extensions;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Abstractions;

namespace Voidwell.Deadlock.Data.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly IDbContextFactory<DeadlockDbContext> _dbContextFactory;

    public ItemRepository(IDbContextFactory<DeadlockDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IEnumerable<Item>> GetAllItemsAsync()
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Items.OrderBy(a => a.Name).ToListAsync();
    }

    public async Task<Item?> GetItemByIdAsync(long id)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Items.SingleOrDefaultAsync(a => a.Id == id);
    }

    public async Task UpsertRangeAsync(IEnumerable<Item> entities)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.UpsertAsync(entities);
    }
}
