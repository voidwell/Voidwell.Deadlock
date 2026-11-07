namespace Voidwell.Deadlock.Collections.Collection.Abstract;

/// <summary>
/// Marker for collections of reference data that change with game patches rather than per match,
/// and so are synced into the database on a schedule.
/// </summary>
public interface IStaticCollection
{
}

public interface IStaticCollection<TModel> : IStaticCollection, IDeadlockCollection<TModel> where TModel : class
{
    Task<IEnumerable<TModel>> GetCollectionAsync();
}
