using Voidwell.Deadlock.Collections.Models;

namespace Voidwell.Deadlock.Collections.Collection;

/// <summary>
/// Normal-mode ranked and unranked matches. Unlike the reference-data collections this one is not synced as a whole:
/// it lists recent matches cheaply, then fetches full metadata for chosen matches.
/// </summary>
public interface IMatchCollection
{
    /// <summary>
    /// Lists every match that started at or after <paramref name="since"/>, with match info only (no players).
    /// </summary>
    Task<IReadOnlyList<MatchMetadataModel>> ListMatchesAsync(DateTime since, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches full metadata, including players, items and stats, for the given matches.
    /// </summary>
    IAsyncEnumerable<MatchMetadataModel> GetMatchesAsync(IReadOnlyCollection<long> matchIds, CancellationToken cancellationToken = default);
}
