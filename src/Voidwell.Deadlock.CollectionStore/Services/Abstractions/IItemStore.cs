using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.CollectionStore.Services.Abstractions;

public interface IItemStore
{
    Task<IEnumerable<Item>?> GetAllItems();
    Task<Item?> GetItem(long itemId);
}
