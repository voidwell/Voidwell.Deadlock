using Voidwell.Common.Cache;
using Voidwell.Deadlock.CollectionStore.Services.Abstractions;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Abstractions;

namespace Voidwell.Deadlock.CollectionStore.Services;

public class HeroStore : IHeroStore
{
    private readonly IHeroRepository _heroRepository;
    private readonly ICache _cache;

    private const string _cacheKeyPrefix = "deadlock.herostore";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    public HeroStore(IHeroRepository heroRepository, ICache cache)
    {
        _heroRepository = heroRepository;
        _cache = cache;
    }

    public async Task<IEnumerable<Hero>?> GetAllHeroes()
    {
        return await _cache.GetOrSetAsync(
            _cacheKeyPrefix,
            ct => _heroRepository.GetAllHeroesAsync(),
            _cacheExpiration);
    }

    public async Task<Hero?> GetHero(int heroId)
    {
        return await _cache.GetOrSetAsync(
            $"{_cacheKeyPrefix}_{heroId}",
            ct => _heroRepository.GetHeroByIdAsync(heroId),
            _cacheExpiration);
    }
}
