namespace Voidwell.Deadlock.Collections;

public interface IAssetsClient
{
    /// <summary>
    /// Fetches a JSON array resource (for example <c>v1/assets/heroes</c>) and deserializes it. Returns an empty list when the response body is null.
    /// </summary>
    Task<IEnumerable<T>> GetListAsync<T>(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches a newline-delimited JSON resource and yields each line as it arrives, so large responses are never held in memory at once.
    /// </summary>
    IAsyncEnumerable<T> StreamLinesAsync<T>(string path, CancellationToken cancellationToken = default);
}
