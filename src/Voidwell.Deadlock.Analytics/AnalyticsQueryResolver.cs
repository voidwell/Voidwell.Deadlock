using Voidwell.Deadlock.CollectionStore.Services.Abstractions;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Abstractions;

namespace Voidwell.Deadlock.Analytics;

public interface IAnalyticsQueryResolver
{
    /// <summary>
    /// Validates the request and works out the days, mode and rank tiers it covers.
    /// </summary>
    Task<AnalyticsQuery> ResolveAsync(AnalyticsRequest request);

    /// <summary>
    /// The query to compare <paramref name="current"/> against: the compare parameters if given, otherwise the period of
    /// equal length just before it.
    /// </summary>
    Task<AnalyticsQuery> ResolveBaselineAsync(AnalyticsRequest request, AnalyticsQuery current);
}

public class AnalyticsQueryResolver : IAnalyticsQueryResolver
{
    private readonly IHeroAnalyticsRepository _repository;
    private readonly IPatchStore _patchStore;

    public AnalyticsQueryResolver(IHeroAnalyticsRepository repository, IPatchStore patchStore)
    {
        _repository = repository;
        _patchStore = patchStore;
    }

    public async Task<AnalyticsQuery> ResolveAsync(AnalyticsRequest request)
    {
        var mode = ParseMode(request.Mode);
        var (minTier, maxTier) = ValidateTiers(request.MinTier, request.MaxTier, mode);

        var latest = await GetLatestDayAsync();
        var patches = request.Patch != null ? await GetPatchesAsync() : [];
        var (from, to) = AnalyticsRanges.Resolve(request, latest, patches);

        return new AnalyticsQuery { From = from, To = to, Mode = mode, MinTier = minTier, MaxTier = maxTier };
    }

    public async Task<AnalyticsQuery> ResolveBaselineAsync(AnalyticsRequest request, AnalyticsQuery current)
    {
        DateOnly from, to;

        if (request.ComparePatch != null)
        {
            if (request.CompareFrom != null || request.CompareTo != null)
            {
                throw new AnalyticsRequestException("comparePatch cannot be combined with compareFrom or compareTo.");
            }

            var latest = await GetLatestDayAsync();
            (from, to) = AnalyticsRanges.FromPatch(await GetPatchesAsync(), request.ComparePatch, request.MajorPatchesOnly, latest);
        }
        else if (request.CompareFrom != null || request.CompareTo != null)
        {
            to = request.CompareTo ?? current.From.AddDays(-1);
            from = request.CompareFrom ?? to.AddDays(-(current.DayCount - 1));

            if (from > to)
            {
                throw new AnalyticsRequestException("compareFrom must not be after compareTo.");
            }
        }
        else
        {
            to = current.From.AddDays(-1);
            from = to.AddDays(-(current.DayCount - 1));
        }

        return new AnalyticsQuery { From = from, To = to, Mode = current.Mode, MinTier = current.MinTier, MaxTier = current.MaxTier };
    }

    private static AnalyticsMode ParseMode(string? mode)
    {
        if (string.IsNullOrWhiteSpace(mode))
        {
            return AnalyticsMode.Ranked;
        }

        return Enum.TryParse<AnalyticsMode>(mode, ignoreCase: true, out var parsed) && Enum.IsDefined(parsed)
            ? parsed
            : throw new AnalyticsRequestException("mode must be ranked, unranked or all.");
    }

    private static (int? Min, int? Max) ValidateTiers(int? minTier, int? maxTier, AnalyticsMode mode)
    {
        if (minTier == null && maxTier == null)
        {
            return (null, null);
        }

        if (mode != AnalyticsMode.Ranked)
        {
            throw new AnalyticsRequestException("minTier and maxTier only apply to ranked matches, because unranked matches have no rank.");
        }

        var min = minTier ?? 0;
        var max = maxTier ?? AnalyticsQuery.HighestTier;

        if (min < 0 || max > AnalyticsQuery.HighestTier || min > max)
        {
            throw new AnalyticsRequestException($"Tiers must satisfy 0 <= minTier <= maxTier <= {AnalyticsQuery.HighestTier}.");
        }

        return (min, max);
    }

    private async Task<DateOnly> GetLatestDayAsync()
    {
        return await _repository.GetLatestDayAsync() ?? DateOnly.FromDateTime(DateTime.UtcNow);
    }

    private async Task<IReadOnlyList<Patch>> GetPatchesAsync()
    {
        return (await _patchStore.GetAllPatches())?.ToList() ?? [];
    }
}
