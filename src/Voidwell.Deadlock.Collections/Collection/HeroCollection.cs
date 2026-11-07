using Voidwell.Deadlock.Collections.Collection.Abstract;
using Voidwell.Deadlock.Collections.Models;

namespace Voidwell.Deadlock.Collections.Collection;

public class HeroCollection : IStaticCollection<AssetHeroModel>
{
    private readonly IAssetsClient _client;

    public string CollectionName => "heroes";

    public HeroCollection(IAssetsClient client)
    {
        _client = client;
    }

    public Task<IEnumerable<AssetHeroModel>> GetCollectionAsync()
    {
        return _client.GetListAsync<AssetHeroModel>("v1/assets/heroes");
    }
}
