using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Voidwell.Deadlock.Collections.Collection.Abstract;
using Voidwell.Deadlock.Data;
using Voidwell.Deadlock.Data.Extensions;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Abstractions;
using Voidwell.Deadlock.Utils.HostedService;

namespace Voidwell.Deadlock.CollectionStore.StoreUpdater;

public class StoreUpdaterScheduler : IStoreUpdaterService, IStatefulHostedService
{
    private readonly IUpdaterSchedulerRepository _updaterSchedulerRepository;
    private readonly IDbContextFactory<DeadlockDbContext> _dbContextFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly StaticStoreUpdaterConfiguration _options;
    private readonly ILogger<StoreUpdaterScheduler> _logger;
    private readonly ICollectionToEntityMapper _mapper;

    private readonly Dictionary<Type, LastStoreUpdate> _collectionLog = new();
    private readonly Dictionary<Type, Timer> _updaterTimers = new();

    private readonly SemaphoreSlim _updateLock = new(1);

    public StoreUpdaterScheduler(IUpdaterSchedulerRepository updaterSchedulerRepository, IDbContextFactory<DeadlockDbContext> dbContextFactory,
        IServiceProvider serviceProvider, IOptions<StaticStoreUpdaterConfiguration> options, ILogger<StoreUpdaterScheduler> logger,
        ICollectionToEntityMapper mapper)
    {
        _updaterSchedulerRepository = updaterSchedulerRepository;
        _dbContextFactory = dbContextFactory;
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task OnApplicationStartup(CancellationToken cancellationToken)
    {
        foreach (var updateConfig in _options.Collections)
        {
            var updaterHistory = await _updaterSchedulerRepository.GetUpdaterHistoryByServiceNameAsync(updateConfig.StoreName!);

            UpdateServiceLog(updateConfig, updaterHistory?.LastUpdateDate);
        }

        // Start with the application unless DisableUpdater is set. Without this the timers would only run after someone
        // enabled the service through the API, and that setting is lost on restart when there is no shared cache.
        await StartAsync(cancellationToken);
    }

    public Task OnApplicationShutdown(CancellationToken cancellationToken)
    {
        return StopAsync(cancellationToken);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return Task.CompletedTask;
        }

        foreach (var collectionType in _collectionLog.Keys)
        {
            RegisterUpdaterTimer(collectionType);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _updaterTimers.ToList().ForEach(a => a.Value?.Dispose());
        _updaterTimers.Clear();

        return Task.CompletedTask;
    }

    public IEnumerable<LastStoreUpdate> GetStoreUpdateLog()
    {
        return _collectionLog.Values.OrderBy(a => a.StoreName).ToList();
    }

    public Task<object> GetStatusAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<object>($"Tracking {_collectionLog.Count} updatable collections");
    }

    public async Task<LastStoreUpdate?> UpdateStoreAsync(string storeName)
    {
        var updateConfig = _options?.Collections?.FirstOrDefault(a => a.StoreName == storeName);
        if (updateConfig == null)
        {
            return null;
        }

        await RunStoreUpdateAsync(updateConfig.CollectionType!);

        return _collectionLog[updateConfig.CollectionType!];
    }

    private async Task<bool> RunStoreUpdateAsync(Type collectionType)
    {
        var updateConfig = _options.Collections.First(a => a.CollectionType == collectionType);

        if (updateConfig.Dependencies != null)
        {
            foreach (var dependency in updateConfig.Dependencies)
            {
                if (!await RunStoreUpdateAsync(dependency))
                {
                    return false;
                }
            }
        }

        await _updateLock.WaitAsync();

        try
        {
            if (_collectionLog[collectionType].LastUpdated != null && DateTime.UtcNow - _collectionLog[collectionType].LastUpdated < TimeSpan.FromSeconds(60))
            {
                return true;
            }

            _logger.LogInformation("Updating {StoreName}.", updateConfig.StoreName);

            var collectionValues = await GetCollectionValuesAsync(updateConfig.CollectionType!);
            if (collectionValues == null)
            {
                return false;
            }

            var entityValues = MapEntityValues(updateConfig.EntityType!, collectionValues);

            await UpdateEntitiesAsync(updateConfig, entityValues);

            _logger.LogInformation("Update complete for {StoreName}.", updateConfig.StoreName);

            var dataModel = new UpdaterScheduler
            {
                Id = updateConfig.StoreName,
                LastUpdateDate = DateTime.UtcNow
            };
            await _updaterSchedulerRepository.UpsertAsync(dataModel);

            UpdateServiceLog(updateConfig, dataModel.LastUpdateDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update failed for {StoreName}", updateConfig.StoreName);
            return false;
        }
        finally
        {
            _updateLock.Release();
        }

        return true;
    }

    private async Task UpdateEntitiesAsync(StaticStoreUpdateConfiguration updateConfig, object values)
    {
        if (updateConfig.UpdateHandlerType == null)
        {
            await UpsertAsync(values);
            return;
        }

        var handler = _serviceProvider.GetRequiredService(updateConfig.UpdateHandlerType);

        var methodInfo = updateConfig.UpdateHandlerType.GetMethod(nameof(IStoreUpdateHandler<object>.UpdateAsync));
        var task = (Task)methodInfo!.Invoke(handler, new[] { values })!;

        await task.WaitAsync(CancellationToken.None);
    }

    private readonly MethodInfo _upsertMethod = typeof(DbSetExtensions).GetMethods()
                .First(a => a.Name == "UpsertAsync" && a.GetParameters()[1].ParameterType.IsGenericType && typeof(IEnumerable<>).IsAssignableTo(a.GetParameters()[1].ParameterType.GetGenericTypeDefinition()));
    private async Task UpsertAsync(object values)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var task = (Task)_upsertMethod.MakeGenericMethod(values.GetType().GetGenericArguments()[0])
            .Invoke(dbContext, new[] { dbContext, values })!;

        await task!.WaitAsync(CancellationToken.None);
    }

    private object MapEntityValues(Type entityType, object values)
    {
        // The collection returns an IEnumerable<TModel>, whatever concrete list type that is.
        var valueType = values.GetType().GetInterfaces()
            .First(a => a.IsGenericType && a.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            .GetGenericArguments()[0];

        return _mapper.Map((System.Collections.IEnumerable)values, valueType, entityType);
    }

    private async Task<object?> GetCollectionValuesAsync(Type staticCollectionType)
    {
        var updaterService = _serviceProvider.GetRequiredService(staticCollectionType);

        var methodInfo = updaterService.GetType().GetMethod(nameof(IStaticCollection<object>.GetCollectionAsync));
        var task = (Task)methodInfo!.Invoke(updaterService, null)!;

        await task!.WaitAsync(CancellationToken.None);

        var result = task.GetType().GetProperty(nameof(Task<object>.Result))!.GetValue(task);

        return result;
    }

    private void RegisterUpdaterTimer(Type collectionType)
    {
        if (_updaterTimers.ContainsKey(collectionType))
        {
            _updaterTimers[collectionType].Dispose();
        }

        var updateConfig = _options.Collections.First(a => a.CollectionType == collectionType);

        var remainingInterval = TimeSpan.Zero;
        if (_collectionLog[collectionType].LastUpdated != null)
        {
            var offset = _collectionLog[collectionType].LastUpdated!.Value.Add(updateConfig.Period) - DateTime.UtcNow;
            if (offset.TotalMilliseconds > 0)
            {
                remainingInterval = offset;
            }
        }

        _updaterTimers[collectionType] = new Timer(HandleTimerAsync, updateConfig.CollectionType, remainingInterval, updateConfig.Period);
    }

    private async void HandleTimerAsync(object? stateInfo)
    {
        await RunStoreUpdateAsync((stateInfo as Type)!);
    }

    private void UpdateServiceLog(StaticStoreUpdateConfiguration updateConfig, DateTime? updateTime)
    {
        _collectionLog[updateConfig.CollectionType!] = new LastStoreUpdate
        {
            StoreName = updateConfig.StoreName,
            LastUpdated = updateTime,
            UpdateInterval = updateConfig.Period
        };
    }
}
