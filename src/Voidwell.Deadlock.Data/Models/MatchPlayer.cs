namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// One player's result in one match. Stored as a TimescaleDB hypertable partitioned on <see cref="StartTime"/>.
/// </summary>
public class MatchPlayer
{
    public long MatchId { get; set; }
    public DateTime StartTime { get; set; }
    public int PlayerSlot { get; set; }

    public long AccountId { get; set; }
    public Team Team { get; set; }
    public int HeroId { get; set; }
    public bool Won { get; set; }
    public PlayerMatchOutcome PlayerMatchOutcome { get; set; }

    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int NetWorth { get; set; }
    public int LastHits { get; set; }
    public int Denies { get; set; }
    public int AbilityPoints { get; set; }
    public int PlayerLevel { get; set; }
    public int HeroXp { get; set; }
    public int? MvpRank { get; set; }
    public int AbandonMatchTimeS { get; set; }

    public long? Party { get; set; }
    public int AssignedLane { get; set; }
    public long? HeroBuildId { get; set; }
    public int? PregameHeroId { get; set; }

    // Copied from the match so common analytics filters do not need a join.
    public MatchMode MatchMode { get; set; }
    public int DurationS { get; set; }
    public int? AverageBadge { get; set; }
}
