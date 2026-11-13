namespace Voidwell.Deadlock.Analytics;

public class RollupOptions
{
    /// <summary>
    /// Minutes between rollup runs.
    /// </summary>
    public int RollupIntervalMinutes { get; set; } = 60;

    /// <summary>
    /// How many of the newest days with matches each run recomputes. Matches reach the database after their day has
    /// ended, so the last days keep changing for a while.
    /// </summary>
    public int RollupRecomputeDays { get; set; } = 3;
}
