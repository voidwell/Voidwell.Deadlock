using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Abstractions;
using Voidwell.Deadlock.Data.Repositories.Models;

namespace Voidwell.Deadlock.Data.Repositories;

public partial class MatchRepository : IMatchRepository
{
    private const string _matchCopyCommand = "COPY match (match_id, start_time, duration_s, match_mode, winning_team, average_badge_team0, " +
        "average_badge_team1, average_badge, banned_hero_ids, objectives_mask_team0, objectives_mask_team1, is_high_skill_range_parties, " +
        "low_pri_pool, new_player_pool, not_scored, game_mode_version) FROM STDIN (FORMAT BINARY)";

    private const string _playerCopyCommand = "COPY match_player (match_id, start_time, player_slot, account_id, team, hero_id, won, " +
        "player_match_outcome, kills, deaths, assists, net_worth, last_hits, denies, ability_points, player_level, hero_xp, mvp_rank, " +
        "abandon_match_time_s, party, assigned_lane, hero_build_id, pregame_hero_id, match_mode, duration_s, average_badge) FROM STDIN (FORMAT BINARY)";

    private const string _itemCopyCommand = "COPY match_player_item (match_id, start_time, player_slot, seq, item_id, upgrade_id, " +
        "game_time_s, sold_time_s, flags, imbued_ability_id) FROM STDIN (FORMAT BINARY)";

    private readonly IDbContextFactory<DeadlockDbContext> _dbContextFactory;

    public MatchRepository(IDbContextFactory<DeadlockDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<DateTime?> GetLatestStartTimeAsync(CancellationToken cancellationToken = default)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Matches.MaxAsync(a => (DateTime?)a.StartTime, cancellationToken);
    }

    public async Task<HashSet<long>> GetExistingMatchIdsAsync(IReadOnlyCollection<long> matchIds, DateTime minStartTime, CancellationToken cancellationToken = default)
    {
        if (matchIds.Count == 0)
        {
            return [];
        }

        var ids = matchIds.ToList();

        using var dbContext = _dbContextFactory.CreateDbContext();
        var existing = await dbContext.Matches
            .Where(a => a.StartTime >= minStartTime && ids.Contains(a.MatchId))
            .Select(a => a.MatchId)
            .ToListAsync(cancellationToken);

        return existing.ToHashSet();
    }

    public async Task InsertAsync(IReadOnlyCollection<MatchRecord> records, CancellationToken cancellationToken = default)
    {
        if (records.Count == 0)
        {
            return;
        }

        using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);

        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await CopyAsync(connection, _matchCopyCommand, records.Select(a => a.Match), WriteMatchAsync, cancellationToken);
        await CopyAsync(connection, _playerCopyCommand, records.SelectMany(a => a.Players), WritePlayerAsync, cancellationToken);
        await CopyAsync(connection, _itemCopyCommand, records.SelectMany(a => a.Items), WriteItemAsync, cancellationToken);
        await CopyAsync(connection, _statCopyCommand, records.SelectMany(a => a.Stats), WriteStatAsync, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    private static async Task CopyAsync<T>(NpgsqlConnection connection, string command, IEnumerable<T> rows,
        Func<NpgsqlBinaryImporter, T, CancellationToken, Task> writeRow, CancellationToken cancellationToken)
    {
        await using var writer = await connection.BeginBinaryImportAsync(command, cancellationToken);

        foreach (var row in rows)
        {
            await writeRow(writer, row, cancellationToken);
        }

        await writer.CompleteAsync(cancellationToken);
    }

    private static async Task WriteMatchAsync(NpgsqlBinaryImporter writer, Match match, CancellationToken cancellationToken)
    {
        await writer.StartRowAsync(cancellationToken);
        await writer.WriteAsync(match.MatchId, NpgsqlDbType.Bigint, cancellationToken);
        await writer.WriteAsync(match.StartTime, NpgsqlDbType.TimestampTz, cancellationToken);
        await writer.WriteAsync(match.DurationS, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync((short)match.MatchMode, NpgsqlDbType.Smallint, cancellationToken);
        await writer.WriteAsync((short)match.WinningTeam, NpgsqlDbType.Smallint, cancellationToken);
        await WriteNullableAsync(writer, match.AverageBadgeTeam0, NpgsqlDbType.Integer, cancellationToken);
        await WriteNullableAsync(writer, match.AverageBadgeTeam1, NpgsqlDbType.Integer, cancellationToken);
        await WriteNullableAsync(writer, match.AverageBadge, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(match.BannedHeroIds, NpgsqlDbType.Array | NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(match.ObjectivesMaskTeam0, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(match.ObjectivesMaskTeam1, NpgsqlDbType.Integer, cancellationToken);
        await WriteNullableAsync(writer, match.IsHighSkillRangeParties, NpgsqlDbType.Boolean, cancellationToken);
        await WriteNullableAsync(writer, match.LowPriPool, NpgsqlDbType.Boolean, cancellationToken);
        await WriteNullableAsync(writer, match.NewPlayerPool, NpgsqlDbType.Boolean, cancellationToken);
        await WriteNullableAsync(writer, match.NotScored, NpgsqlDbType.Boolean, cancellationToken);
        await WriteNullableAsync(writer, match.GameModeVersion, NpgsqlDbType.Integer, cancellationToken);
    }

    private static async Task WritePlayerAsync(NpgsqlBinaryImporter writer, MatchPlayer player, CancellationToken cancellationToken)
    {
        await writer.StartRowAsync(cancellationToken);
        await writer.WriteAsync(player.MatchId, NpgsqlDbType.Bigint, cancellationToken);
        await writer.WriteAsync(player.StartTime, NpgsqlDbType.TimestampTz, cancellationToken);
        await writer.WriteAsync(player.PlayerSlot, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(player.AccountId, NpgsqlDbType.Bigint, cancellationToken);
        await writer.WriteAsync((short)player.Team, NpgsqlDbType.Smallint, cancellationToken);
        await writer.WriteAsync(player.HeroId, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(player.Won, NpgsqlDbType.Boolean, cancellationToken);
        await writer.WriteAsync((short)player.PlayerMatchOutcome, NpgsqlDbType.Smallint, cancellationToken);
        await writer.WriteAsync(player.Kills, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(player.Deaths, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(player.Assists, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(player.NetWorth, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(player.LastHits, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(player.Denies, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(player.AbilityPoints, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(player.PlayerLevel, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(player.HeroXp, NpgsqlDbType.Integer, cancellationToken);
        await WriteNullableAsync(writer, player.MvpRank, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(player.AbandonMatchTimeS, NpgsqlDbType.Integer, cancellationToken);
        await WriteNullableAsync(writer, player.Party, NpgsqlDbType.Bigint, cancellationToken);
        await writer.WriteAsync(player.AssignedLane, NpgsqlDbType.Integer, cancellationToken);
        await WriteNullableAsync(writer, player.HeroBuildId, NpgsqlDbType.Bigint, cancellationToken);
        await WriteNullableAsync(writer, player.PregameHeroId, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync((short)player.MatchMode, NpgsqlDbType.Smallint, cancellationToken);
        await writer.WriteAsync(player.DurationS, NpgsqlDbType.Integer, cancellationToken);
        await WriteNullableAsync(writer, player.AverageBadge, NpgsqlDbType.Integer, cancellationToken);
    }

    private static async Task WriteItemAsync(NpgsqlBinaryImporter writer, MatchPlayerItem item, CancellationToken cancellationToken)
    {
        await writer.StartRowAsync(cancellationToken);
        await writer.WriteAsync(item.MatchId, NpgsqlDbType.Bigint, cancellationToken);
        await writer.WriteAsync(item.StartTime, NpgsqlDbType.TimestampTz, cancellationToken);
        await writer.WriteAsync(item.PlayerSlot, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(item.Seq, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(item.ItemId, NpgsqlDbType.Bigint, cancellationToken);
        await writer.WriteAsync(item.UpgradeId, NpgsqlDbType.Bigint, cancellationToken);
        await writer.WriteAsync(item.GameTimeS, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(item.SoldTimeS, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(item.Flags, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(item.ImbuedAbilityId, NpgsqlDbType.Bigint, cancellationToken);
    }

    private static async Task WriteNullableAsync<T>(NpgsqlBinaryImporter writer, T? value, NpgsqlDbType type, CancellationToken cancellationToken)
        where T : struct
    {
        if (value.HasValue)
        {
            await writer.WriteAsync(value.Value, type, cancellationToken);
        }
        else
        {
            await writer.WriteNullAsync(cancellationToken);
        }
    }
}
