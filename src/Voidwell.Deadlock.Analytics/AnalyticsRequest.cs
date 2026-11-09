namespace Voidwell.Deadlock.Analytics;

/// <summary>
/// The query parameters shared by the analytics endpoints. Everything is optional.
/// </summary>
public class AnalyticsRequest
{
    /// <summary>
    /// First UTC day, inclusive.
    /// </summary>
    public DateOnly? From { get; set; }

    /// <summary>
    /// Last UTC day, inclusive. Defaults to the newest day with data.
    /// </summary>
    public DateOnly? To { get; set; }

    /// <summary>
    /// How many days to cover, ending at <see cref="To"/>. Defaults to 30 when no range is given.
    /// </summary>
    public int? Days { get; set; }

    /// <summary>
    /// A patch release date (yyyy-MM-dd) or "latest". Covers the days from the day after that patch to the day before
    /// the next one. Cannot be combined with <see cref="From"/>, <see cref="To"/> or <see cref="Days"/>.
    /// </summary>
    public string? Patch { get; set; }

    /// <summary>
    /// When true, only major patch days start and end a patch range.
    /// </summary>
    public bool MajorPatchesOnly { get; set; }

    /// <summary>
    /// ranked (the default), unranked or all.
    /// </summary>
    public string? Mode { get; set; }

    /// <summary>
    /// Lowest and highest rank tier (0 to 11, see the rank endpoint) of ranked matches. Setting either limits the query to ranked.
    /// </summary>
    public int? MinTier { get; set; }
    public int? MaxTier { get; set; }

    // The baseline for the trends endpoint. When none is given, it is the period of equal length just before the range.
    public DateOnly? CompareFrom { get; set; }
    public DateOnly? CompareTo { get; set; }
    public string? ComparePatch { get; set; }
}
