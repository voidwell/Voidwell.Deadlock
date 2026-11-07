using Voidwell.Common.Cache;
using Voidwell.Deadlock.CollectionStore.Services.Abstractions;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Abstractions;

namespace Voidwell.Deadlock.CollectionStore.Services;

public class ItemStore : IItemStore
{
    private readonly IItemRepository _itemRepository;
    private readonly ICache _cache;

    private const string _cacheKeyPrefix = "deadlock.itemstore";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    public ItemStore(IItemRepository itemRepository, ICache cache)
    {
        _itemRepository = itemRepository;
        _cache = cache;
    }

    public async Task<IEnumerable<Item>?> GetAllItems()
    {
        return await _cache.GetOrSetAsync(
            _cacheKeyPrefix,
            ct => _itemRepository.GetAllItemsAsync(),
            _cacheExpiration);
    }

    public async Task<Item?> GetItem(long itemId)
    {
        return await _cache.GetOrSetAsync(
            $"{_cacheKeyPrefix}_{itemId}",
            ct => _itemRepository.GetItemByIdAsync(itemId),
            _cacheExpiration);
    }
}
