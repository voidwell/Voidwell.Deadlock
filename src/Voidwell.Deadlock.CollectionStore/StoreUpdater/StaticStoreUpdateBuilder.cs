using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Voidwell.Deadlock.Collections.Collection.Abstract;

namespace Voidwell.Deadlock.CollectionStore.StoreUpdater;

public class StaticStoreUpdateBuilder<TCollection, TEntity> : IStaticStoreUpdateBuilder<TCollection>
    where TCollection : class, IStaticCollection
    where TEntity : class
{
    private readonly IServiceCollection _serviceCollection;

    public StaticStoreUpdateBuilder(IServiceCollection serviceCollection, bool enabled = true, int periodDays = 7)
    {
        _serviceCollection = serviceCollection;

        _serviceCollection.Configure<StaticStoreUpdaterConfiguration>(updaterConfig =>
        {
            if (updaterConfig.Collections.Any(a => a.CollectionType == typeof(TCollection) || a.EntityType == typeof(TEntity)))
            {
                throw new ArgumentException($"Updater collection conflict: Collection type '{typeof(TCollection).Name}' or entity type '{typeof(TEntity).Name}' is already registered.");
            }

            var updateConfig = new StaticStoreUpdateConfiguration
            {
                Enabled = enabled,
                CollectionType = typeof(TCollection),
                EntityType = typeof(TEntity),
                StoreName = typeof(TCollection).Name.Replace("Collection", "Store"),
                Period = TimeSpan.FromDays(periodDays)
            };

            updaterConfig.Collections.Add(updateConfig);
        });
    }

    public IStaticStoreUpdateBuilder<TCollection> WithUpdateDependency<TDependency>()
        where TDependency : class, IStaticCollection
    {
        if (!typeof(IStaticCollection).IsAssignableFrom(typeof(TDependency)))
        {
            throw new ArgumentException($"Store update configuration dependency '{typeof(TDependency).Name}' for collection '{typeof(TCollection).Name}' does not implement {nameof(IStaticCollection)}.");
        }

        if (typeof(TDependency) == typeof(TCollection))
        {
            throw new ArgumentException($"Store update collection '{typeof(TCollection)}' cannot depend on itself.");
        }

        _serviceCollection.PostConfigure<StaticStoreUpdaterConfiguration>(updaterConfig =>
        {
            var dependency = updaterConfig.Collections.FirstOrDefault(a => a.CollectionType == typeof(TDependency));
            if (dependency == null)
            {
                throw new ArgumentException($"Store update collection dependency '{typeof(TDependency)}' is not a registered update store.");
            }

            CyclicalDependencyCheck(updaterConfig.Collections, typeof(TDependency));

            updaterConfig.Collections.First(a => a.CollectionType == typeof(TCollection)).Dependencies.Add(typeof(TDependency));
        });

        return this;
    }

    public IStaticStoreUpdateBuilder<TCollection> WithUpdateHandler<THandler>()
        where THandler : class
    {
        if (!typeof(IStoreUpdateHandler<TEntity>).IsAssignableFrom(typeof(THandler)))
        {
            throw new ArgumentException($"Store update handler '{typeof(THandler).Name}' for collection '{typeof(TCollection).Name}' does not implement {nameof(IStoreUpdateHandler<TEntity>)}<{typeof(TEntity).Name}>.");
        }

        _serviceCollection.TryAddTransient<THandler>();

        _serviceCollection.PostConfigure<StaticStoreUpdaterConfiguration>(updaterConfig =>
        {
            updaterConfig.Collections.First(a => a.CollectionType == typeof(TCollection)).UpdateHandlerType = typeof(THandler);
        });

        return this;
    }

    private static void CyclicalDependencyCheck(List<StaticStoreUpdateConfiguration> updateConfigs, Type dependencyType)
    {
        var cyclicalPath = FindCyclicalPath(updateConfigs, typeof(TCollection), dependencyType);
        if (cyclicalPath != null)
        {
            var depChain = string.Join(" -> ", cyclicalPath.Select(a => a.Name));
            throw new ArgumentException($"Cyclical update dependency detected: {depChain}");
        }
    }

    private static IEnumerable<Type>? FindCyclicalPath(List<StaticStoreUpdateConfiguration> updateConfigs, Type sourceType, Type dependencyType)
    {
        var dependencyConfig = updateConfigs.FirstOrDefault(a => a.CollectionType == dependencyType);

        if (dependencyConfig!.Dependencies != null)
        {
            foreach (var subDependency in dependencyConfig.Dependencies)
            {
                if (subDependency == sourceType)
                {
                    return new[] { subDependency };
                }

                var cyclicalPath = FindCyclicalPath(updateConfigs, sourceType, subDependency);
                if (cyclicalPath != null)
                {
                    return cyclicalPath.Append(subDependency);
                }
            }
        }

        return null;
    }
}
