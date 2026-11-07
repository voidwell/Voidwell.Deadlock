using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.CollectionStore.Services.Abstractions;

public interface IHeroStore
{
    Task<IEnumerable<Hero>?> GetAllHeroes();
    Task<Hero?> GetHero(int heroId);
}
