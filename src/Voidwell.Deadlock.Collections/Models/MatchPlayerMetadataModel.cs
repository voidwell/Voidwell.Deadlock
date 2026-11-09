namespace Voidwell.Deadlock.Collections.Models;

public class MatchPlayerMetadataModel
{
    public long AccountId { get; set; }
    public int HeroId { get; set; }
    public int PlayerSlot { get; set; }
    public string? Team { get; set; }
    public long Kills { get; set; }
    public long Deaths { get; set; }
    public long Assists { get; set; }
    public long NetWorth { get; set; }
    public long LastHits { get; set; }
    public long Denies { get; set; }
    public long AbilityPoints { get; set; }
    public long AssignedLane { get; set; }
    public long PlayerLevel { get; set; }
    public long AbandonMatchTimeS { get; set; }
    public int? MvpRank { get; set; }
    public string? PlayerMatchOutcome { get; set; }

    // Only returned when requested with extra_player_columns, and absent when the value is unknown.
    public long? HeroXp { get; set; }
    public bool? Won { get; set; }
    public long? HeroBuildId { get; set; }
    public long? Party { get; set; }
    public int? PregameHeroId { get; set; }

    public List<MatchItemMetadataModel>? Items { get; set; }
    public List<MatchStatMetadataModel>? Stats { get; set; }
}
