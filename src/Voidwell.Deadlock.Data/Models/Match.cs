namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// One match. Stored as a TimescaleDB hypertable partitioned on <see cref="StartTime"/>.
/// </summary>
public class Match
{
    public long MatchId { get; set; }
    public DateTime StartTime { get; set; }
    public int DurationS { get; set; }
    public MatchMode MatchMode { get; set; }
    public Team WinningTeam { get; set; }

    /// <summary>
    /// Average badge of each team, or of the whole match, as tier * 10 + subtier. Null for matches without ranked data.
    /// </summary>
    public int? AverageBadgeTeam0 { get; set; }
    public int? AverageBadgeTeam1 { get; set; }
    public int? AverageBadge { get; set; }

    public int[] BannedHeroIds { get; set; } = [];

    public int ObjectivesMaskTeam0 { get; set; }
    public int ObjectivesMaskTeam1 { get; set; }
    public bool? IsHighSkillRangeParties { get; set; }
    public bool? LowPriPool { get; set; }
    public bool? NewPlayerPool { get; set; }
    public bool? NotScored { get; set; }
    public int? GameModeVersion { get; set; }
}
