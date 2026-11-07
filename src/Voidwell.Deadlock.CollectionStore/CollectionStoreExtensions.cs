using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Voidwell.Deadlock.Collections;
using Voidwell.Deadlock.Collections.Collection;
using Voidwell.Deadlock.CollectionStore.Services;
using Voidwell.Deadlock.CollectionStore.Services.Abstractions;
using Voidwell.Deadlock.CollectionStore.StoreUpdater;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Utils.HostedService;

namespace Voidwell.Deadlock.CollectionStore;

public static class CollectionStoreExtensions
{
    public static IServiceCollection AddCollectionStores(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();
        services.Configure<StoreOptions>(configuration);

        services.AddDeadlockCollections();

        services.AddSingleton<ICollectionToEntityMapper, CollectionToEntityMapper>();

        services.AddSingleton<IHeroStore, HeroStore>();
        services.AddSingleton<IItemStore, ItemStore>();
        services.AddSingleton<IRankStore, RankStore>();

        services.AddStatefulHostedService<IStoreUpdaterService, StoreUpdaterScheduler>();

        services.Configure<StaticStoreUpdaterConfiguration>(o => o.Enabled = !configuration.GetValue<bool>("DisableUpdater"));

        services.AddUpdaterStaticStore<HeroCollection, Hero>();
        services.AddUpdaterStaticStore<ItemCollection, Item>();
        services.AddUpdaterStaticStore<RankCollection, Rank>();

        return services;
    }
}
