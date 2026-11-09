using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Voidwell.Deadlock.Data.Repositories.Abstractions;

namespace Voidwell.Deadlock.Analytics;

public interface IRollupService
{
    /// <summary>
    /// Recomputes the newest days with matches, and any days since the last rollup. Returns how many days were rebuilt.
    /// </summary>
    Task<int> UpdateRecentAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Recomputes every day in the range, oldest first, reporting each finished day to <paramref name="onDayCompleted"/>.
    /// </summary>
    Task RebuildAsync(DateOnly from, DateOnly to, Action<DateOnly>? onDayCompleted, CancellationToken cancellationToken);
}

public class RollupService : IRollupService
{
    private readonly IRollupRepository _rollupRepository;
    private readonly RollupOptions _options;
    private readonly ILogger<RollupService> _logger;

    public RollupService(IRollupRepository rollupRepository, IOptions<RollupOptions> options, ILogger<RollupService> logger)
    {
        _rollupRepository = rollupRepository;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<int> UpdateRecentAsync(CancellationToken cancellationToken)
    {
        var matchDays = await _rollupRepository.GetMatchDayRangeAsync(cancellationToken);
        if (matchDays == null)
        {
            return 0;
        }

        var recompute = Math.Max(1, _options.RollupRecomputeDays);
        var latestRollup = await _rollupRepository.GetLatestRollupDayAsync(cancellationToken);

        // Without any rollups only the newest days are built here. Building history is a deliberate step (a rebuild),
        // because it should wait until the history has finished loading.
        var from = (latestRollup ?? matchDays.Value.Last).AddDays(-(recompute - 1));
        var to = matchDays.Value.Last;

        if (from > to)
        {
            return 0;
        }

        await RebuildAsync(from, to, null, cancellationToken);

        return to.DayNumber - from.DayNumber + 1;
    }

    public async Task RebuildAsync(DateOnly from, DateOnly to, Action<DateOnly>? onDayCompleted, CancellationToken cancellationToken)
    {
        for (var day = from; day <= to; day = day.AddDays(1))
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _rollupRepository.RebuildDayAsync(day, cancellationToken);
            onDayCompleted?.Invoke(day);

            _logger.LogInformation("Rolled up {Day:yyyy-MM-dd}.", day);
        }
    }
}
