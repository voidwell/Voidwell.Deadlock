using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.Repositories.Abstractions;

public interface IPatchRepository
{
    /// <summary>
    /// All patches, oldest first.
    /// </summary>
    Task<IEnumerable<Patch>> GetAllPatchesAsync();

    Task UpsertRangeAsync(IEnumerable<Patch> entities);
}
