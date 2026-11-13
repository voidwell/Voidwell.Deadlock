using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Voidwell.Common.State;

namespace Voidwell.Deadlock.Analytics;

/// <summary>
/// Keeps the rollup tables current on a timer, and runs full rebuilds on request. Rebuilds can run while the timer is
/// disabled.
/// </summary>
public class RollupUpdater : StatefulHostedService
{
    private readonly IRollupService _rollupService;
    private readonly RollupOptions _options;
    private readonly ILogger<RollupUpdater> _logger;

    // Only one rollup job runs at a time, whether it is a scheduled update or a rebuild.
    private readonly SemaphoreSlim _runLock = new(1, 1);
    private readonly object _timerLock = new();
    private Timer? _timer;
    private CancellationTokenSource? _stopping;

    private DateTime? _lastRunUtc;
    private int _lastDaysRebuilt;
    private string? _lastError;
    private string? _rebuildProgress;

    public RollupUpdater(IRollupService rollupService, IOptions<RollupOptions> options, ILogger<RollupUpdater> logger)
    {
        _rollupService = rollupService;
        _options = options.Value;
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        StartTimer();
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        StopTimer();
        return Task.CompletedTask;
    }

    public override Task<object?> GetStatusAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<object?>(new
        {
            LastSuccessfulRunUtc = _lastRunUtc,
            LastDaysRebuilt = _lastDaysRebuilt,
            LastError = _lastError,
            Rebuild = _rebuildProgress
        });
    }

    /// <summary>
    /// Starts a rebuild of every day in the range in the background. Returns false if a rollup job is already running.
    /// </summary>
    public bool TryStartRebuild(DateOnly from, DateOnly to)
    {
        if (!_runLock.Wait(0))
        {
            return false;
        }

        _rebuildProgress = $"Rebuilding {from:yyyy-MM-dd} to {to:yyyy-MM-dd}: starting";

        _ = Task.Run(async () =>
        {
            try
            {
                await _rollupService.RebuildAsync(from, to, day => _rebuildProgress = $"Rebuilding {from:yyyy-MM-dd} to {to:yyyy-MM-dd}: done through {day:yyyy-MM-dd}", CancellationToken.None);

                _rebuildProgress = $"Rebuilt {from:yyyy-MM-dd} to {to:yyyy-MM-dd}";
                _lastError = null;
            }
            catch (Exception ex)
            {
                _rebuildProgress = $"Rebuild of {from:yyyy-MM-dd} to {to:yyyy-MM-dd} failed";
                _lastError = ex.Message;
                _logger.LogError(ex, "Rollup rebuild failed.");
            }
            finally
            {
                _runLock.Release();
            }
        });

        return true;
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        if (!await _runLock.WaitAsync(0, cancellationToken))
        {
            return;
        }

        try
        {
            var runStart = DateTime.UtcNow;
            _lastDaysRebuilt = await _rollupService.UpdateRecentAsync(cancellationToken);
            _lastRunUtc = runStart;
            _lastError = null;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _lastError = ex.Message;
            _logger.LogError(ex, "Rollup update failed.");
        }
        finally
        {
            _runLock.Release();
        }
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
            var interval = TimeSpan.FromMinutes(Math.Max(1, _options.RollupIntervalMinutes));
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
        catch (OperationCanceledException)
        {
            // The service is stopping.
        }
    }
}
