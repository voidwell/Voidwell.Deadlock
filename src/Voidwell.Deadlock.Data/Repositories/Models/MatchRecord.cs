using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.Repositories.Models;

/// <summary>
/// A match with everything stored for it, written to the database as one unit.
/// </summary>
public class MatchRecord
{
    public required Match Match { get; init; }
    public List<MatchPlayer> Players { get; } = [];
    public List<MatchPlayerItem> Items { get; } = [];
    public List<MatchPlayerStat> Stats { get; } = [];
}
