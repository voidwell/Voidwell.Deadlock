using Voidwell.Deadlock.Collections.Collection.Abstract;
using Voidwell.Deadlock.Collections.Models;

namespace Voidwell.Deadlock.Collections.Collection;

public class RankCollection : IStaticCollection<AssetRankModel>
{
    private readonly IAssetsClient _client;

    public string CollectionName => "ranks";

    public RankCollection(IAssetsClient client)
    {
        _client = client;
    }

    public Task<IEnumerable<AssetRankModel>> GetCollectionAsync()
    {
        return _client.GetListAsync<AssetRankModel>("v1/assets/ranks");
    }
}
