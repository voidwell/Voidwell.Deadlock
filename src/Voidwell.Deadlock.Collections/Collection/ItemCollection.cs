using Voidwell.Deadlock.Collections.Collection.Abstract;
using Voidwell.Deadlock.Collections.Models;

namespace Voidwell.Deadlock.Collections.Collection;

public class ItemCollection : IStaticCollection<AssetItemModel>
{
    private readonly IAssetsClient _client;

    public string CollectionName => "items";

    public ItemCollection(IAssetsClient client)
    {
        _client = client;
    }

    public Task<IEnumerable<AssetItemModel>> GetCollectionAsync()
    {
        return _client.GetListAsync<AssetItemModel>("v1/assets/items");
    }
}
