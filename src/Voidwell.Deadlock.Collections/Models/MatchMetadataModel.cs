using System.Text.Json.Serialization;

namespace Voidwell.Deadlock.Collections.Models;

/// <summary>
/// One match from /v1/matches/metadata. Enums arrive as strings (for example Team0, Unranked) and times as
/// "yyyy-MM-dd HH:mm:ss" in UTC. <see cref="Players"/> is only filled when player data is requested.
/// </summary>
public class MatchMetadataModel
{
    public long MatchId { get; set; }
    public string? StartTime { get; set; }
    public int DurationS { get; set; }
    public string? WinningTeam { get; set; }
    public string? MatchMode { get; set; }
    public string? GameMode { get; set; }

    [JsonPropertyName("average_badge_team0")]
    public int? AverageBadgeTeam0 { get; set; }

    [JsonPropertyName("average_badge_team1")]
    public int? AverageBadgeTeam1 { get; set; }

    public int? AverageBadge { get; set; }
    public bool? NotScored { get; set; }

    // The columns below are only returned when requested with extra_match_columns.
    public int[]? BannedHeroIds { get; set; }

    [JsonPropertyName("objectives_mask_team0")]
    public int? ObjectivesMaskTeam0 { get; set; }

    [JsonPropertyName("objectives_mask_team1")]
    public int? ObjectivesMaskTeam1 { get; set; }

    public bool? IsHighSkillRangeParties { get; set; }
    public bool? LowPriPool { get; set; }
    public bool? NewPlayerPool { get; set; }
    public int? GameModeVersion { get; set; }

    public List<MatchPlayerMetadataModel>? Players { get; set; }
}
