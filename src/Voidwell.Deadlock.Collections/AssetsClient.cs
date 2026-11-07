using System.Net.Http.Json;
using System.Text.Json;

namespace Voidwell.Deadlock.Collections;

public class AssetsClient : IAssetsClient
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public AssetsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<T>> GetListAsync<T>(string path, CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<T>>(path, _jsonOptions, cancellationToken);

        return result ?? new List<T>();
    }
}
