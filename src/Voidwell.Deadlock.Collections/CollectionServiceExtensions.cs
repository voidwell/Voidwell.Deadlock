using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Voidwell.Deadlock.Collections.Collection;

namespace Voidwell.Deadlock.Collections;

public static class CollectionServiceExtensions
{
    private const string _clientBaseUrl = "https://api.deadlock-api.com/";
    private const string _userAgent = "Voidwell.com";
    private readonly static TimeSpan _clientTimeout = TimeSpan.FromSeconds(120);

    public static IServiceCollection AddDeadlockCollections(this IServiceCollection services)
    {
        services.AddHttpClient<IAssetsClient, AssetsClient>((sp, client) =>
        {
            client.BaseAddress = new Uri(_clientBaseUrl);
            client.Timeout = _clientTimeout;
            client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
        });

        services.TryAddTransient<HeroCollection>();
        services.TryAddTransient<ItemCollection>();
        services.TryAddTransient<RankCollection>();

        return services;
    }
}
