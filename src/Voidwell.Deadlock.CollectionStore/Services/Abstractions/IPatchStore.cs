using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.CollectionStore.Services.Abstractions;

public interface IPatchStore
{
    /// <summary>
    /// All patches, oldest first.
    /// </summary>
    Task<IEnumerable<Patch>?> GetAllPatches();
}
