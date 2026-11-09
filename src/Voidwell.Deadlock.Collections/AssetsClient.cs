using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Voidwell.Deadlock.Collections;

public class AssetsClient : IAssetsClient
{
    public static readonly JsonSerializerOptions JsonOptions = new()
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
        var result = await _httpClient.GetFromJsonAsync<List<T>>(path, JsonOptions, cancellationToken);

        return result ?? new List<T>();
    }

    public async IAsyncEnumerable<T> StreamLinesAsync<T>(string path, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(path, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var item = JsonSerializer.Deserialize<T>(line, JsonOptions);
            if (item != null)
            {
                yield return item;
            }
        }
    }
}
