namespace Voidwell.Deadlock.Analytics;

/// <summary>
/// The caller's query parameters are not valid. The message is safe to return to the caller.
/// </summary>
public class AnalyticsRequestException : Exception
{
    public AnalyticsRequestException(string message)
        : base(message)
    {
    }
}
