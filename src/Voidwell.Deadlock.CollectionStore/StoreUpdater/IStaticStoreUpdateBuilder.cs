using Voidwell.Deadlock.Collections.Collection.Abstract;

namespace Voidwell.Deadlock.CollectionStore.StoreUpdater;

public interface IStaticStoreUpdateBuilder<TCollection>
    where TCollection : class, IStaticCollection
{
    IStaticStoreUpdateBuilder<TCollection> WithUpdateDependency<TDependency>() where TDependency : class, IStaticCollection;
    IStaticStoreUpdateBuilder<TCollection> WithUpdateHandler<THandler>() where THandler : class;
}
