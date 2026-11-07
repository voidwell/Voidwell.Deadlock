using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.Repositories.Abstractions;

public interface IRankRepository
{
    Task<IEnumerable<Rank>> GetAllRanksAsync();
    Task UpsertRangeAsync(IEnumerable<Rank> entities);
}
