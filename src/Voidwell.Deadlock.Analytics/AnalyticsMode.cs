namespace Voidwell.Deadlock.Analytics;

/// <summary>
/// Which matches an analytics query covers. Only ranked matches have a rank, so rank filters apply to ranked alone.
/// </summary>
public enum AnalyticsMode
{
    Ranked,
    Unranked,
    All
}
