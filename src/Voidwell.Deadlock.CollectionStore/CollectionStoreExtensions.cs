using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Voidwell.Common.State;
using Voidwell.Deadlock.Collections;
using Voidwell.Deadlock.Collections.Collection;
using Voidwell.Deadlock.CollectionStore.Services;
using Voidwell.Deadlock.CollectionStore.Services.Abstractions;
using Voidwell.Deadlock.CollectionStore.StoreUpdater;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.CollectionStore;

public static class CollectionStoreExtensions
{
    public static IServiceCollection AddCollectionStores(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();

        services.AddDeadlockCollections();

        services.Configure<MatchUpdaterOptions>(configuration);

        services.AddSingleton<ICollectionToEntityMapper, CollectionToEntityMapper>();

        services.AddSingleton<IHeroStore, HeroStore>();
        services.AddSingleton<IItemStore, ItemStore>();
        services.AddSingleton<IRankStore, RankStore>();
        services.AddSingleton<IPatchStore, PatchStore>();

        services.AddStatefulHostedService<IStoreUpdaterService, StoreUpdaterScheduler>();
        services.AddStatefulHostedService<MatchUpdater>();

        services.AddUpdaterStaticStore<HeroCollection, Hero>();
        services.AddUpdaterStaticStore<ItemCollection, Item>();
        services.AddUpdaterStaticStore<RankCollection, Rank>();

        // The patch feed only lists the latest entries, so it is collected daily to build up the history.
        services.AddUpdaterStaticStore<PatchCollection, Patch>(periodDays: 1);

        return services;
    }
}
