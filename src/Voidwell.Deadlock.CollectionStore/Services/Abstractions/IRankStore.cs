using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.CollectionStore.Services.Abstractions;

public interface IRankStore
{
    Task<IEnumerable<Rank>?> GetAllRanks();
}
