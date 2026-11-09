namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// Matches that started on one UTC day, split by match mode and badge tier. Rollup of <see cref="Match"/>.
/// </summary>
public class MatchDay : IRollupRow
{
    public const short UnrankedTier = -1;

    public DateOnly Day { get; set; }

    /// <summary>
    /// The match mode: 1 for unranked, 4 for ranked.
    /// </summary>
    public short Mode { get; set; }

    /// <summary>
    /// Average badge divided by 10, or <see cref="UnrankedTier"/> when the match has no badge (all unranked matches).
    /// </summary>
    public short Tier { get; set; }

    public int Matches { get; set; }

    /// <summary>
    /// Matches that recorded at least one banned hero. Bans are not recorded in every match, so ban rates divide by this.
    /// </summary>
    public int MatchesWithBans { get; set; }

    public int WinsTeam0 { get; set; }
    public long DurationSumS { get; set; }
}
