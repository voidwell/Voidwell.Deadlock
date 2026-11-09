namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// Matches grouped by how far one team led the other on total souls at one sample time, on one UTC day, and how often the
/// team in front won.
/// </summary>
public class TeamLeadDay : IRollupRow
{
    /// <summary>
    /// Leads of this many thousand souls or more share the last bucket.
    /// </summary>
    public const int MaxLeadBucket = 30;

    public DateOnly Day { get; set; }
    public short Mode { get; set; }
    public short Tier { get; set; }
    public int TimeStampS { get; set; }

    /// <summary>
    /// The lead in thousands of souls, rounded down: 0 for under 1,000, up to <see cref="MaxLeadBucket"/>.
    /// </summary>
    public short LeadBucket { get; set; }

    /// <summary>
    /// Matches where the teams had a stat sample from the same number of players and were not exactly level.
    /// </summary>
    public int Games { get; set; }

    /// <summary>
    /// Of those, matches won by the team that was ahead.
    /// </summary>
    public int LeaderWins { get; set; }
}
