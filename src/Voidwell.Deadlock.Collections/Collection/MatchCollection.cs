using Voidwell.Deadlock.Collections.Models;

namespace Voidwell.Deadlock.Collections.Collection;

public class MatchCollection : IMatchCollection
{
    // Matches the filters used when loading the data dump, so the app and the dump hold the same kind of match.
    private const string _filters = "game_mode=normal&match_mode=ranked,unranked";

    // Columns the match tables keep that the API only returns on request.
    private const string _extraMatchColumns = "banned_hero_ids,objectives_mask_team0,objectives_mask_team1,is_high_skill_range_parties,low_pri_pool,new_player_pool,game_mode_version";
    private const string _extraPlayerColumns = "hero_xp,won,hero_build_id,party,pregame_hero_id";

    private const int _listPageSize = 10000;

    private readonly IAssetsClient _client;

    public MatchCollection(IAssetsClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<MatchMetadataModel>> ListMatchesAsync(DateTime since, CancellationToken cancellationToken = default)
    {
        var sinceUnix = new DateTimeOffset(DateTime.SpecifyKind(since, DateTimeKind.Utc)).ToUnixTimeSeconds();
        var matches = new List<MatchMetadataModel>();
        long minMatchId = 0;

        // The endpoint has no cursor, so pages are walked in match id order using min_match_id.
        while (true)
        {
            var path = $"v1/matches/metadata?format=ndjson&include_info=true&{_filters}" +
                $"&min_unix_timestamp={sinceUnix}&min_match_id={minMatchId}" +
                $"&order_by=match_id&order_direction=asc&limit={_listPageSize}";

            var pageCount = 0;
            await foreach (var match in _client.StreamLinesAsync<MatchMetadataModel>(path, cancellationToken))
            {
                matches.Add(match);
                minMatchId = match.MatchId + 1;
                pageCount++;
            }

            if (pageCount < _listPageSize)
            {
                return matches;
            }
        }
    }

    public IAsyncEnumerable<MatchMetadataModel> GetMatchesAsync(IReadOnlyCollection<long> matchIds, CancellationToken cancellationToken = default)
    {
        var path = $"v1/matches/metadata?format=ndjson&include_info=true&{_filters}" +
            "&include_player_info=true&include_player_items=true&include_player_stats=true" +
            $"&extra_match_columns={_extraMatchColumns}&extra_player_columns={_extraPlayerColumns}" +
            $"&match_ids={string.Join(',', matchIds)}&limit={Math.Max(matchIds.Count, 1)}";

        return _client.StreamLinesAsync<MatchMetadataModel>(path, cancellationToken);
    }
}
