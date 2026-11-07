using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.Repositories.Abstractions;

public interface IItemRepository
{
    Task<IEnumerable<Item>> GetAllItemsAsync();
    Task<Item?> GetItemByIdAsync(long id);
    Task UpsertRangeAsync(IEnumerable<Item> entities);
}
