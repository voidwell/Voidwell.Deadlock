using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.Repositories.Abstractions;

public interface IHeroRepository
{
    Task<IEnumerable<Hero>> GetAllHeroesAsync();
    Task<Hero?> GetHeroByIdAsync(int id);
    Task UpsertRangeAsync(IEnumerable<Hero> entities);
}
