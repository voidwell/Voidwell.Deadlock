using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Voidwell.Deadlock.Collections.Collection;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Abstractions;
using Voidwell.Deadlock.Data.Repositories.Models;
using Voidwell.Deadlock.Utils.HostedService;

namespace Voidwell.Deadlock.CollectionStore.StoreUpdater;

/// <summary>
/// Keeps the match tables current. Each run lists the matches that started in a trailing window, works out which are
/// not stored yet, and fetches and writes those.
/// </summary>
public class MatchUpdater : IStatefulHostedService
{
    public const string SchedulerId = "MatchUpdater";

    private readonly IMatchCollection _matchCollection;
    private readonly IMatchRepository _matchRepository;
    private readonly IUpdaterSchedulerRepository _updaterSchedulerRepository;
    private readonly MatchUpdaterOptions _options;
    private readonly ILogger<MatchUpdater> _logger;

    private readonly SemaphoreSlim _runLock = new(1, 1);
    private readonly object _timerLock = new();
    private Timer? _timer;
    private CancellationTokenSource? _stopping;

    private DateTime? _lastRunUtc;
    private int _lastMatchesAdded;
    private string? _lastError;

    public MatchUpdater(IMatchCollection matchCollection, IMatchRepository matchRepository,
        IUpdaterSchedulerRepository updaterSchedulerRepository, IOptions<MatchUpdaterOptions> options, ILogger<MatchUpdater> logger)
    {
        _matchCollection = matchCollection;
        _matchRepository = matchRepository;
        _updaterSchedulerRepository = updaterSchedulerRepository;
        _options = options.Value;
        _logger = logger;
    }

    public async Task OnApplicationStartup(CancellationToken cancellationToken)
    {
        var history = await _updaterSchedulerRepository.GetUpdaterHistoryByServiceNameAsync(SchedulerId);
        _lastRunUtc = history?.LastUpdateDate;

        if (!_options.DisableMatchUpdater)
        {
            StartTimer();
        }
    }

    public Task OnApplicationShutdown(CancellationToken cancellationToken)
    {
        StopTimer();
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        StartTimer();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        StopTimer();
        return Task.CompletedTask;
    }

    public Task<object> GetStatusAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<object>(new
        {
            LastSuccessfulRunUtc = _lastRunUtc,
            LastMatchesAdded = _lastMatchesAdded,
            LastError = _lastError
        });
    }

    /// <summary>
    /// Runs one update. Returns the number of matches added, or null if a run was already in progress.
    /// </summary>
    public async Task<int?> RunOnceAsync(CancellationToken cancellationToken)
    {
        if (!await _runLock.WaitAsync(0, cancellationToken))
        {
            return null;
        }

        try
        {
            var runStart = DateTime.UtcNow;
            var since = await GetWindowStartAsync(runStart, cancellationToken);

            var listed = await _matchCollection.ListMatchesAsync(since, cancellationToken);
            var listedIds = listed.Select(a => a.MatchId).ToList();

            var existing = await _matchRepository.GetExistingMatchIdsAsync(listedIds, since, cancellationToken);
            var missing = listedIds.Where(a => !existing.Contains(a)).ToList();

            _logger.LogInformation("Match update: {Listed} matches since {Since:u}, {Missing} not stored yet.", listedIds.Count, since, missing.Count);

            var added = 0;
            foreach (var batch in missing.Chunk(Math.Max(1, _options.MatchUpdateBatchSize)))
            {
                added += await FetchAndStoreAsync(batch, cancellationToken);
            }

            // Only a fully completed run moves the window forward, so a failed run is retried over the same window.
            await _updaterSchedulerRepository.UpsertAsync(new UpdaterScheduler { Id = SchedulerId, LastUpdateDate = runStart });

            _lastRunUtc = runStart;
            _lastMatchesAdded = added;
            _lastError = null;

            _logger.LogInformation("Match update complete: {Added} matches added.", added);

            return added;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _lastError = ex.Message;
            _logger.LogError(ex, "Match update failed.");
            throw;
        }
        finally
        {
            _runLock.Release();
        }
    }

    private async Task<DateTime> GetWindowStartAsync(DateTime runStart, CancellationToken cancellationToken)
    {
        var basis = _lastRunUtc
            ?? await _matchRepository.GetLatestStartTimeAsync(cancellationToken)
            ?? runStart.AddHours(-_options.MatchUpdateInitialLookbackHours);

        return DateTime.SpecifyKind(basis, DateTimeKind.Utc).AddMinutes(-_options.MatchUpdateOverlapMinutes);
    }

    private async Task<int> FetchAndStoreAsync(IReadOnlyCollection<long> matchIds, CancellationToken cancellationToken)
    {
        var records = new List<MatchRecord>(matchIds.Count);

        await foreach (var match in _matchCollection.GetMatchesAsync(matchIds, cancellationToken))
        {
            records.Add(MatchMapper.Map(match));
        }

        await _matchRepository.InsertAsync(records, cancellationToken);

        return records.Count;
    }

    private void StartTimer()
    {
        lock (_timerLock)
        {
            if (_timer != null)
            {
                return;
            }

            _stopping = new CancellationTokenSource();
            var interval = TimeSpan.FromMinutes(Math.Max(1, _options.MatchUpdateIntervalMinutes));
            _timer = new Timer(HandleTimerAsync, null, TimeSpan.Zero, interval);
        }
    }

    private void StopTimer()
    {
        lock (_timerLock)
        {
            _timer?.Dispose();
            _timer = null;

            _stopping?.Cancel();
            _stopping?.Dispose();
            _stopping = null;
        }
    }

    private async void HandleTimerAsync(object? state)
    {
        var stopping = _stopping;
        if (stopping == null)
        {
            return;
        }

        try
        {
            await RunOnceAsync(stopping.Token);
        }
        catch (Exception)
        {
            // Already logged and recorded by RunOnceAsync. The next tick retries.
        }
    }
}
