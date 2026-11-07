using Voidwell.Common.Cache;
using Voidwell.Deadlock.CollectionStore.Services.Abstractions;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Abstractions;

namespace Voidwell.Deadlock.CollectionStore.Services;

public class RankStore : IRankStore
{
    private readonly IRankRepository _rankRepository;
    private readonly ICache _cache;

    private const string _cacheKeyPrefix = "deadlock.rankstore";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(6);

    public RankStore(IRankRepository rankRepository, ICache cache)
    {
        _rankRepository = rankRepository;
        _cache = cache;
    }

    public async Task<IEnumerable<Rank>?> GetAllRanks()
    {
        return await _cache.GetOrSetAsync(
            _cacheKeyPrefix,
            ct => _rankRepository.GetAllRanksAsync(),
            _cacheExpiration);
    }
}
