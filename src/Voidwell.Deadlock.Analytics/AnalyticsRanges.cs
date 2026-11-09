using System.Globalization;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Analytics;

/// <summary>
/// Turns the date parameters of a request into an inclusive range of UTC days.
/// </summary>
public static class AnalyticsRanges
{
    public const int DefaultDays = 30;
    public const int MaxDays = 3650;

    public static (DateOnly From, DateOnly To) Resolve(AnalyticsRequest request, DateOnly latest, IReadOnlyList<Patch> patches)
    {
        var hasDates = request.From != null || request.To != null || request.Days != null;

        if (request.Patch == null)
        {
            return FromDates(request.From, request.To, request.Days, latest);
        }

        if (hasDates)
        {
            throw new AnalyticsRequestException("patch cannot be combined with from, to or days.");
        }

        return FromPatch(patches, request.Patch, request.MajorPatchesOnly, latest);
    }

    public static (DateOnly From, DateOnly To) FromDates(DateOnly? from, DateOnly? to, int? days, DateOnly latest)
    {
        if (days is < 1 or > MaxDays)
        {
            throw new AnalyticsRequestException($"days must be between 1 and {MaxDays}.");
        }

        if (from != null && days != null)
        {
            throw new AnalyticsRequestException("Use either from or days, not both.");
        }

        var end = to ?? latest;
        var start = from ?? end.AddDays(-((days ?? DefaultDays) - 1));

        if (start > end)
        {
            throw new AnalyticsRequestException("from must not be after to.");
        }

        return (start, end);
    }

    /// <summary>
    /// A patch covers the days after its release day up to the day before the next release. The release days themselves
    /// are left out because they are split between the old and new patch, and the rollups are daily.
    /// </summary>
    public static (DateOnly From, DateOnly To) FromPatch(IReadOnlyList<Patch> patches, string patch, bool majorOnly, DateOnly latest)
    {
        var releases = patches
            .Where(a => !majorOnly || a.IsBigDay)
            .Select(a => DateOnly.FromDateTime(a.PublishedAt.ToUniversalTime()))
            .Distinct()
            .Order()
            .ToList();

        if (releases.Count == 0)
        {
            throw new AnalyticsRequestException("No patches are known yet.");
        }

        DateOnly release;
        if (patch.Equals("latest", StringComparison.OrdinalIgnoreCase))
        {
            release = releases[^1];
        }
        else if (DateOnly.TryParseExact(patch, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out release))
        {
            if (!releases.Contains(release))
            {
                throw new AnalyticsRequestException($"No patch was released on {release:yyyy-MM-dd}. See the patch endpoint for release dates.");
            }
        }
        else
        {
            throw new AnalyticsRequestException("patch must be a release date (yyyy-MM-dd) or \"latest\".");
        }

        var index = releases.IndexOf(release);
        var from = release.AddDays(1);
        var to = index + 1 < releases.Count ? releases[index + 1].AddDays(-1) : latest;

        if (to > latest)
        {
            to = latest;
        }

        if (from > to)
        {
            throw new AnalyticsRequestException($"The patch released on {release:yyyy-MM-dd} has no full days of data yet.");
        }

        return (from, to);
    }
}
