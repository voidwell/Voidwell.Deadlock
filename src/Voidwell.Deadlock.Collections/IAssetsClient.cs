namespace Voidwell.Deadlock.Collections;

public interface IAssetsClient
{
    /// <summary>
    /// Fetches a JSON array resource (for example <c>v2/heroes</c>) and deserializes it. Returns an empty list when the response body is null.
    /// </summary>
    Task<IEnumerable<T>> GetListAsync<T>(string path, CancellationToken cancellationToken = default);
}
