using Voidwell.Common.Cache;
using Voidwell.Deadlock.CollectionStore.Services.Abstractions;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Abstractions;

namespace Voidwell.Deadlock.CollectionStore.Services;

public class PatchStore : IPatchStore
{
    private readonly IPatchRepository _patchRepository;
    private readonly ICache _cache;

    private const string _cacheKeyPrefix = "deadlock.patchstore";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);

    public PatchStore(IPatchRepository patchRepository, ICache cache)
    {
        _patchRepository = patchRepository;
        _cache = cache;
    }

    public async Task<IEnumerable<Patch>?> GetAllPatches()
    {
        return await _cache.GetOrSetAsync(
            _cacheKeyPrefix,
            ct => _patchRepository.GetAllPatchesAsync(),
            _cacheExpiration);
    }
}
