using Voidwell.Deadlock.Data.Repositories.Models;

namespace Voidwell.Deadlock.Data.Repositories.Abstractions;

public interface IMatchRepository
{
    /// <summary>
    /// The start time of the newest stored match, or null when no matches are stored.
    /// </summary>
    Task<DateTime?> GetLatestStartTimeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Which of the given matches are already stored. <paramref name="minStartTime"/> must not be later than any of the
    /// matches' start times; it lets the database skip older partitions.
    /// </summary>
    Task<HashSet<long>> GetExistingMatchIdsAsync(IReadOnlyCollection<long> matchIds, DateTime minStartTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the matches, their players, items and stats in one transaction. Fails if a match is already stored, so
    /// callers check <see cref="GetExistingMatchIdsAsync"/> first.
    /// </summary>
    Task InsertAsync(IReadOnlyCollection<MatchRecord> records, CancellationToken cancellationToken = default);
}
