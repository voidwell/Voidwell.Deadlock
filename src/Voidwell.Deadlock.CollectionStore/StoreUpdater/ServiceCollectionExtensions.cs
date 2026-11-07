using Microsoft.Extensions.DependencyInjection;
using Voidwell.Deadlock.Collections.Collection.Abstract;

namespace Voidwell.Deadlock.CollectionStore.StoreUpdater;

internal static class ServiceCollectionExtensions
{
    public static IStaticStoreUpdateBuilder<TCollection> AddUpdaterStaticStore<TCollection, TEntity>(this IServiceCollection services, bool enabled = true, int periodDays = 7)
        where TCollection : class, IStaticCollection
        where TEntity : class
    {
        return new StaticStoreUpdateBuilder<TCollection, TEntity>(services, enabled, periodDays);
    }
}
