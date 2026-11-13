namespace Voidwell.Deadlock.CollectionStore;

public class MatchUpdaterOptions
{
    /// <summary>
    /// Minutes between update runs.
    /// </summary>
    public int MatchUpdateIntervalMinutes { get; set; } = 5;

    /// <summary>
    /// Each run looks back this far before the last successful run. A match only appears in the API once it has ended
    /// and its metadata is fetched, so matches that started before the last run can still turn up afterwards.
    /// </summary>
    public int MatchUpdateOverlapMinutes { get; set; } = 120;

    /// <summary>
    /// How far back the first run looks when the database is empty and no run has been recorded.
    /// </summary>
    public int MatchUpdateInitialLookbackHours { get; set; } = 24;

    /// <summary>
    /// Matches fetched, with their players, items and stats, per API request.
    /// </summary>
    public int MatchUpdateBatchSize { get; set; } = 100;
}
