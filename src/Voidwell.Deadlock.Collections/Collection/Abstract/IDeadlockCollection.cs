namespace Voidwell.Deadlock.Collections.Collection.Abstract;

public interface IDeadlockCollection<TModel> : IDeadlockCollection where TModel : class
{
}

public interface IDeadlockCollection
{
    public string CollectionName { get; }
}
