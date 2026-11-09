using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using Voidwell.Deadlock.Data.Repositories.Abstractions;

namespace Voidwell.Deadlock.Data.Repositories;

public class RollupRepository : IRollupRepository
{
    // Every statement filters the raw tables to one UTC day. Written as a time range so the hypertables can skip
    // all other chunks, and pinned to UTC so the database's time zone setting cannot move the boundaries.
    private const string _dayStart = "(@day::timestamp AT TIME ZONE 'UTC')";
    private const string _dayEnd = "((@day::timestamp + interval '1 day') AT TIME ZONE 'UTC')";

    private const string _deleteSql = """
        DELETE FROM match_day WHERE day = @day;
        DELETE FROM hero_day WHERE day = @day;
        DELETE FROM hero_ban_day WHERE day = @day;
        DELETE FROM hero_item_day WHERE day = @day;
        DELETE FROM item_day WHERE day = @day;
        DELETE FROM hero_build_shape_day WHERE day = @day;
        DELETE FROM hero_build_day WHERE day = @day;
        DELETE FROM hero_ability_day WHERE day = @day;
        DELETE FROM hero_ability_pick_day WHERE day = @day;
        DELETE FROM hero_matchup_day WHERE day = @day;
        DELETE FROM hero_synergy_day WHERE day = @day;
        DELETE FROM hero_lane_day WHERE day = @day;
        DELETE FROM team_composition_day WHERE day = @day;
        DELETE FROM hero_curve_day WHERE day = @day;
        DELETE FROM team_lead_day WHERE day = @day;
        DELETE FROM match_hour_day WHERE day = @day;
        DELETE FROM match_duration_day WHERE day = @day;
        DELETE FROM match_objective_day WHERE day = @day;
        DELETE FROM match_leaver_day WHERE day = @day;
        """;

    private const string _matchDaySql = $"""
        INSERT INTO match_day (day, mode, tier, matches, matches_with_bans, wins_team0, duration_sum_s)
        SELECT @day, match_mode, COALESCE(average_badge / 10, -1),
               count(*),
               count(*) FILTER (WHERE cardinality(banned_hero_ids) > 0),
               count(*) FILTER (WHERE winning_team = 0),
               sum(duration_s)
        FROM match
        WHERE start_time >= {_dayStart} AND start_time < {_dayEnd}
        GROUP BY match_mode, COALESCE(average_badge / 10, -1)
        """;

    // A player's last stat sample holds their final totals for the match.
    private const string _heroDaySql = $"""
        INSERT INTO hero_day (day, mode, tier, hero_id, games, wins, kills, deaths, assists, net_worth, last_hits, denies,
                              duration_sum_s, stat_games, player_damage, player_damage_taken, boss_damage, player_healing,
                              shots_hit, shots_missed)
        WITH final_stats AS (
            SELECT DISTINCT ON (match_id, player_slot)
                   match_id, player_slot, player_damage, player_damage_taken, boss_damage, player_healing, shots_hit, shots_missed
            FROM match_player_stat
            WHERE start_time >= {_dayStart} AND start_time < {_dayEnd}
            ORDER BY match_id, player_slot, time_stamp_s DESC
        )
        SELECT @day, p.match_mode, COALESCE(p.average_badge / 10, -1), p.hero_id,
               count(*),
               count(*) FILTER (WHERE p.won),
               sum(p.kills), sum(p.deaths), sum(p.assists), sum(p.net_worth), sum(p.last_hits), sum(p.denies),
               sum(p.duration_s),
               count(f.match_id),
               COALESCE(sum(f.player_damage), 0), COALESCE(sum(f.player_damage_taken), 0), COALESCE(sum(f.boss_damage), 0),
               COALESCE(sum(f.player_healing), 0), COALESCE(sum(f.shots_hit), 0), COALESCE(sum(f.shots_missed), 0)
        FROM match_player p
        LEFT JOIN final_stats f ON f.match_id = p.match_id AND f.player_slot = p.player_slot
        WHERE p.start_time >= {_dayStart} AND p.start_time < {_dayEnd}
        GROUP BY p.match_mode, COALESCE(p.average_badge / 10, -1), p.hero_id
        """;

    private const string _heroBanDaySql = $"""
        INSERT INTO hero_ban_day (day, mode, tier, hero_id, bans)
        SELECT @day, m.match_mode, COALESCE(m.average_badge / 10, -1), b.hero_id, count(*)
        FROM match m
        CROSS JOIN LATERAL unnest(m.banned_hero_ids) AS b(hero_id)
        WHERE m.start_time >= {_dayStart} AND m.start_time < {_dayEnd}
        GROUP BY m.match_mode, COALESCE(m.average_badge / 10, -1), b.hero_id
        """;

    // Item rollups. A "game" is a player who bought the item at least once; a player who sold it and bought it again still
    // counts once. Only shop upgrades are counted here, abilities are handled below.
    private const string _heroItemDaySql = $"""
        INSERT INTO hero_item_day (hero_id, day, mode, tier, item_id, games, wins, held_games, held_wins, buy_time_sum_s,
                                   early_games, early_wins, mid_games, mid_wins, late_games, late_wins, endgame_games, endgame_wins)
        WITH purchases AS (
            SELECT i.match_id, i.player_slot, i.item_id,
                   min(i.game_time_s) AS first_buy_s,
                   bool_or(i.sold_time_s = 0) AS held
            FROM match_player_item i
            JOIN item r ON r.id = i.item_id AND r.type = 'upgrade'
            WHERE i.start_time >= {_dayStart} AND i.start_time < {_dayEnd}
            GROUP BY i.match_id, i.player_slot, i.item_id
        )
        SELECT p.hero_id, @day, p.match_mode, COALESCE(p.average_badge / 10, -1), u.item_id,
               count(*), count(*) FILTER (WHERE p.won),
               count(*) FILTER (WHERE u.held), count(*) FILTER (WHERE u.held AND p.won),
               sum(u.first_buy_s),
               count(*) FILTER (WHERE u.first_buy_s < 600),
               count(*) FILTER (WHERE u.first_buy_s < 600 AND p.won),
               count(*) FILTER (WHERE u.first_buy_s >= 600 AND u.first_buy_s < 1200),
               count(*) FILTER (WHERE u.first_buy_s >= 600 AND u.first_buy_s < 1200 AND p.won),
               count(*) FILTER (WHERE u.first_buy_s >= 1200 AND u.first_buy_s < 1800),
               count(*) FILTER (WHERE u.first_buy_s >= 1200 AND u.first_buy_s < 1800 AND p.won),
               count(*) FILTER (WHERE u.first_buy_s >= 1800),
               count(*) FILTER (WHERE u.first_buy_s >= 1800 AND p.won)
        FROM purchases u
        JOIN match_player p ON p.match_id = u.match_id AND p.player_slot = u.player_slot
        WHERE p.start_time >= {_dayStart} AND p.start_time < {_dayEnd}
        GROUP BY p.hero_id, p.match_mode, COALESCE(p.average_badge / 10, -1), u.item_id
        """;

    private const string _itemDaySql = """
        INSERT INTO item_day (day, mode, tier, item_id, games, wins, held_games, held_wins, buy_time_sum_s,
                              early_games, early_wins, mid_games, mid_wins, late_games, late_wins, endgame_games, endgame_wins)
        SELECT day, mode, tier, item_id,
               sum(games), sum(wins), sum(held_games), sum(held_wins), sum(buy_time_sum_s),
               sum(early_games), sum(early_wins), sum(mid_games), sum(mid_wins),
               sum(late_games), sum(late_wins), sum(endgame_games), sum(endgame_wins)
        FROM hero_item_day
        WHERE day = @day
        GROUP BY day, mode, tier, item_id
        """;

    // Players with no tier 3 or higher item still count, as a build of 0 weapon, 0 spirit and 0 vitality items.
    private const string _heroBuildShapeDaySql = $"""
        INSERT INTO hero_build_shape_day (hero_id, day, mode, tier, weapon_items, spirit_items, vitality_items, games, wins)
        WITH held AS (
            SELECT DISTINCT i.match_id, i.player_slot, i.item_id
            FROM match_player_item i
            WHERE i.start_time >= {_dayStart} AND i.start_time < {_dayEnd} AND i.sold_time_s = 0
        ),
        shape AS (
            SELECT h.match_id, h.player_slot,
                   count(*) FILTER (WHERE r.slot_type = 'weapon') AS weapon_items,
                   count(*) FILTER (WHERE r.slot_type = 'spirit') AS spirit_items,
                   count(*) FILTER (WHERE r.slot_type = 'vitality') AS vitality_items
            FROM held h
            JOIN item r ON r.id = h.item_id AND r.type = 'upgrade' AND r.tier >= 3
            GROUP BY h.match_id, h.player_slot
        )
        SELECT p.hero_id, @day, p.match_mode, COALESCE(p.average_badge / 10, -1),
               COALESCE(s.weapon_items, 0)::smallint, COALESCE(s.spirit_items, 0)::smallint, COALESCE(s.vitality_items, 0)::smallint,
               count(*), count(*) FILTER (WHERE p.won)
        FROM match_player p
        LEFT JOIN shape s ON s.match_id = p.match_id AND s.player_slot = p.player_slot
        WHERE p.start_time >= {_dayStart} AND p.start_time < {_dayEnd}
        GROUP BY p.hero_id, p.match_mode, COALESCE(p.average_badge / 10, -1),
                 COALESCE(s.weapon_items, 0), COALESCE(s.spirit_items, 0), COALESCE(s.vitality_items, 0)
        """;

    private const string _heroBuildDaySql = $"""
        INSERT INTO hero_build_day (hero_id, day, mode, tier, build_id, games, wins)
        SELECT hero_id, @day, match_mode, COALESCE(average_badge / 10, -1), hero_build_id,
               count(*), count(*) FILTER (WHERE won)
        FROM match_player
        WHERE start_time >= {_dayStart} AND start_time < {_dayEnd} AND hero_build_id IS NOT NULL
        GROUP BY hero_id, match_mode, COALESCE(average_badge / 10, -1), hero_build_id
        """;

    // An ability takes four points, so its fourth point is the earliest it can be maxed.
    private const string _heroAbilityDaySql = $"""
        INSERT INTO hero_ability_day (hero_id, day, mode, tier, ability_id, games, wins, maxed_games, maxed_wins,
                                      max_first_games, max_first_wins, unlock_time_sum_s)
        WITH points AS (
            SELECT i.match_id, i.player_slot, i.item_id, i.game_time_s,
                   row_number() OVER (PARTITION BY i.match_id, i.player_slot, i.item_id ORDER BY i.game_time_s, i.seq) AS point_no
            FROM match_player_item i
            JOIN item r ON r.id = i.item_id AND r.type = 'ability'
            WHERE i.start_time >= {_dayStart} AND i.start_time < {_dayEnd}
        ),
        abilities AS (
            SELECT match_id, player_slot, item_id,
                   min(game_time_s) AS unlock_s,
                   max(game_time_s) FILTER (WHERE point_no = 4) AS maxed_s
            FROM points
            GROUP BY match_id, player_slot, item_id
        ),
        first_maxed AS (
            SELECT DISTINCT ON (match_id, player_slot) match_id, player_slot, item_id
            FROM abilities
            WHERE maxed_s IS NOT NULL
            ORDER BY match_id, player_slot, maxed_s, item_id
        )
        SELECT p.hero_id, @day, p.match_mode, COALESCE(p.average_badge / 10, -1), a.item_id,
               count(*), count(*) FILTER (WHERE p.won),
               count(a.maxed_s), count(*) FILTER (WHERE a.maxed_s IS NOT NULL AND p.won),
               count(f.item_id), count(*) FILTER (WHERE f.item_id IS NOT NULL AND p.won),
               sum(a.unlock_s)
        FROM abilities a
        JOIN match_player p ON p.match_id = a.match_id AND p.player_slot = a.player_slot
        LEFT JOIN first_maxed f ON f.match_id = a.match_id AND f.player_slot = a.player_slot AND f.item_id = a.item_id
        WHERE p.start_time >= {_dayStart} AND p.start_time < {_dayEnd}
        GROUP BY p.hero_id, p.match_mode, COALESCE(p.average_badge / 10, -1), a.item_id
        """;

    private const string _heroAbilityPickDaySql = $"""
        INSERT INTO hero_ability_pick_day (hero_id, day, mode, tier, pick_index, ability_id, games, wins)
        WITH picks AS (
            SELECT i.match_id, i.player_slot, i.item_id,
                   row_number() OVER (PARTITION BY i.match_id, i.player_slot ORDER BY i.game_time_s, i.seq) AS pick_no
            FROM match_player_item i
            JOIN item r ON r.id = i.item_id AND r.type = 'ability'
            WHERE i.start_time >= {_dayStart} AND i.start_time < {_dayEnd}
        )
        SELECT p.hero_id, @day, p.match_mode, COALESCE(p.average_badge / 10, -1), k.pick_no::smallint, k.item_id,
               count(*), count(*) FILTER (WHERE p.won)
        FROM picks k
        JOIN match_player p ON p.match_id = k.match_id AND p.player_slot = k.player_slot
        WHERE p.start_time >= {_dayStart} AND p.start_time < {_dayEnd}
        GROUP BY p.hero_id, p.match_mode, COALESCE(p.average_badge / 10, -1), k.pick_no, k.item_id
        """;

    // Pair rollups join the day's players to themselves within each match. Spectators do not exist in these matches, so
    // "the other team" is simply the team that is not this one.
    private const string _heroMatchupDaySql = $"""
        INSERT INTO hero_matchup_day (hero_id, enemy_hero_id, day, mode, tier, games, wins)
        SELECT a.hero_id, b.hero_id, @day, a.match_mode, COALESCE(a.average_badge / 10, -1),
               count(*), count(*) FILTER (WHERE a.won)
        FROM match_player a
        JOIN match_player b ON b.match_id = a.match_id AND b.start_time = a.start_time AND b.team <> a.team
        WHERE a.start_time >= {_dayStart} AND a.start_time < {_dayEnd}
          AND b.start_time >= {_dayStart} AND b.start_time < {_dayEnd}
        GROUP BY a.hero_id, b.hero_id, a.match_mode, COALESCE(a.average_badge / 10, -1)
        """;

    private const string _heroSynergyDaySql = $"""
        INSERT INTO hero_synergy_day (hero_id, ally_hero_id, day, mode, tier, games, wins)
        SELECT a.hero_id, b.hero_id, @day, a.match_mode, COALESCE(a.average_badge / 10, -1),
               count(*), count(*) FILTER (WHERE a.won)
        FROM match_player a
        JOIN match_player b ON b.match_id = a.match_id AND b.start_time = a.start_time AND b.team = a.team AND b.player_slot <> a.player_slot
        WHERE a.start_time >= {_dayStart} AND a.start_time < {_dayEnd}
          AND b.start_time >= {_dayStart} AND b.start_time < {_dayEnd}
        GROUP BY a.hero_id, b.hero_id, a.match_mode, COALESCE(a.average_badge / 10, -1)
        """;

    // Each lane has two players per team. The lead is measured at nine minutes (the 540 second stat sample), near the end of
    // the laning phase.
    private const string _heroLaneDaySql = $"""
        INSERT INTO hero_lane_day (hero_id, enemy_hero_id, day, mode, tier, games, wins, lead_games, lead_sum)
        WITH nine AS (
            SELECT match_id, player_slot, net_worth
            FROM match_player_stat
            WHERE start_time >= {_dayStart} AND start_time < {_dayEnd} AND time_stamp_s = 540
        )
        SELECT a.hero_id, b.hero_id, @day, a.match_mode, COALESCE(a.average_badge / 10, -1),
               count(*), count(*) FILTER (WHERE a.won),
               count(*) FILTER (WHERE na.net_worth IS NOT NULL AND nb.net_worth IS NOT NULL),
               COALESCE(sum(na.net_worth::bigint - nb.net_worth::bigint) FILTER (WHERE na.net_worth IS NOT NULL AND nb.net_worth IS NOT NULL), 0)
        FROM match_player a
        JOIN match_player b ON b.match_id = a.match_id AND b.start_time = a.start_time AND b.team <> a.team AND b.assigned_lane = a.assigned_lane
        LEFT JOIN nine na ON na.match_id = a.match_id AND na.player_slot = a.player_slot
        LEFT JOIN nine nb ON nb.match_id = b.match_id AND nb.player_slot = b.player_slot
        WHERE a.start_time >= {_dayStart} AND a.start_time < {_dayEnd}
          AND b.start_time >= {_dayStart} AND b.start_time < {_dayEnd}
        GROUP BY a.hero_id, b.hero_id, a.match_mode, COALESCE(a.average_badge / 10, -1)
        """;

    // Heroes without an archetype in the game data are the support heroes, so they count as supports.
    private const string _teamCompositionDaySql = $"""
        INSERT INTO team_composition_day (day, mode, tier, brawlers, assassins, marksmen, mystics, supports, games, wins)
        WITH teams AS (
            SELECT p.match_id, p.team, p.match_mode, COALESCE(p.average_badge / 10, -1) AS tier, bool_or(p.won) AS won,
                   count(*) FILTER (WHERE h.hero_type = 'brawler') AS brawlers,
                   count(*) FILTER (WHERE h.hero_type = 'assassin') AS assassins,
                   count(*) FILTER (WHERE h.hero_type = 'marksman') AS marksmen,
                   count(*) FILTER (WHERE h.hero_type = 'mystic') AS mystics,
                   count(*) FILTER (WHERE h.hero_type IS NULL OR h.hero_type NOT IN ('brawler', 'assassin', 'marksman', 'mystic')) AS supports
            FROM match_player p
            LEFT JOIN hero h ON h.id = p.hero_id
            WHERE p.start_time >= {_dayStart} AND p.start_time < {_dayEnd}
            GROUP BY p.match_id, p.team, p.match_mode, COALESCE(p.average_badge / 10, -1)
        )
        SELECT @day, match_mode, tier, brawlers::smallint, assassins::smallint, marksmen::smallint, mystics::smallint, supports::smallint,
               count(*), count(*) FILTER (WHERE won)
        FROM teams
        GROUP BY match_mode, tier, brawlers, assassins, marksmen, mystics, supports
        """;

    // Stats are sampled every 180 seconds for the first 15 minutes and then every 15 minutes. A few other samples exist
    // (for example the final state of a match), but too few to chart, so only the standard times are summarized.
    private const string _sampleTimes = "(180, 360, 540, 720, 900, 1800, 2700, 3600, 4500)";

    // Orb souls are in addition to the direct souls of a source, so each source adds the two.
    private const string _heroCurveDaySql = $"""
        INSERT INTO hero_curve_day (hero_id, day, mode, tier, won, time_stamp_s, samples, net_worth, level, kills, deaths, assists,
                                    player_damage, player_damage_taken, player_healing, weapon_power, tech_power, max_health,
                                    kill_souls, lane_souls, neutral_souls, boss_souls, treasure_souls)
        SELECT p.hero_id, @day, p.match_mode, COALESCE(p.average_badge / 10, -1), p.won, s.time_stamp_s,
               count(*), sum(s.net_worth), sum(s.level), sum(s.kills), sum(s.deaths), sum(s.assists),
               sum(s.player_damage), sum(s.player_damage_taken), sum(s.player_healing), sum(s.weapon_power), sum(s.tech_power), sum(s.max_health),
               sum(s.gold_player::bigint + s.gold_player_orbs), sum(s.gold_lane_creep::bigint + s.gold_lane_creep_orbs),
               sum(s.gold_neutral_creep::bigint + s.gold_neutral_creep_orbs), sum(s.gold_boss::bigint + s.gold_boss_orb),
               sum(s.gold_treasure::bigint)
        FROM match_player_stat s
        JOIN match_player p ON p.match_id = s.match_id AND p.player_slot = s.player_slot AND p.start_time = s.start_time
        WHERE s.start_time >= {_dayStart} AND s.start_time < {_dayEnd}
          AND p.start_time >= {_dayStart} AND p.start_time < {_dayEnd}
          AND s.time_stamp_s IN {_sampleTimes}
        GROUP BY p.hero_id, p.match_mode, COALESCE(p.average_badge / 10, -1), p.won, s.time_stamp_s
        """;

    // A team's souls are the total net worth of its players. The sum is taken over the integer column so that it stays a
    // bigint: the sum of a bigint is a numeric, and dividing that by 1000 would give fractional buckets.
    // Only samples where both teams have the same number of players
    // are compared, so a missing sample cannot look like a lead.
    private const string _teamLeadDaySql = $"""
        INSERT INTO team_lead_day (day, mode, tier, time_stamp_s, lead_bucket, games, leader_wins)
        WITH teams AS (
            SELECT s.match_id, s.time_stamp_s, p.team, p.match_mode, COALESCE(p.average_badge / 10, -1) AS tier,
                   count(*) AS players, sum(s.net_worth) AS souls, bool_or(p.won) AS won
            FROM match_player_stat s
            JOIN match_player p ON p.match_id = s.match_id AND p.player_slot = s.player_slot AND p.start_time = s.start_time
            WHERE s.start_time >= {_dayStart} AND s.start_time < {_dayEnd}
              AND p.start_time >= {_dayStart} AND p.start_time < {_dayEnd}
              AND s.time_stamp_s IN {_sampleTimes}
            GROUP BY s.match_id, s.time_stamp_s, p.team, p.match_mode, COALESCE(p.average_badge / 10, -1)
        )
        SELECT @day, a.match_mode, a.tier, a.time_stamp_s,
               LEAST(abs(a.souls - b.souls) / 1000, 30)::smallint,
               count(*),
               count(*) FILTER (WHERE (a.souls > b.souls AND a.won) OR (b.souls > a.souls AND b.won))
        FROM teams a
        JOIN teams b ON b.match_id = a.match_id AND b.time_stamp_s = a.time_stamp_s AND a.team = 0 AND b.team = 1
        WHERE a.players = b.players AND a.souls <> b.souls
        GROUP BY a.match_mode, a.tier, a.time_stamp_s, LEAST(abs(a.souls - b.souls) / 1000, 30)
        """;

    private const string _matchHourDaySql = $"""
        INSERT INTO match_hour_day (day, mode, tier, hour, matches)
        SELECT @day, match_mode, COALESCE(average_badge / 10, -1),
               extract(hour FROM start_time AT TIME ZONE 'UTC')::smallint,
               count(*)
        FROM match
        WHERE start_time >= {_dayStart} AND start_time < {_dayEnd}
        GROUP BY match_mode, COALESCE(average_badge / 10, -1), extract(hour FROM start_time AT TIME ZONE 'UTC')
        """;

    private const string _matchDurationDaySql = $"""
        INSERT INTO match_duration_day (day, mode, tier, minute, matches)
        SELECT @day, match_mode, COALESCE(average_badge / 10, -1), LEAST(duration_s / 60, 120)::smallint, count(*)
        FROM match
        WHERE start_time >= {_dayStart} AND start_time < {_dayEnd}
        GROUP BY match_mode, COALESCE(average_badge / 10, -1), LEAST(duration_s / 60, 120)
        """;

    // The game stores the objectives still standing at the end of a match as one bit each, per team. A match with both masks
    // empty has no objective data (the masks default to zero), so it is left out, as is a match nobody won.
    private const string _matchObjectiveDaySql = $"""
        INSERT INTO match_objective_day (day, mode, tier, objective, lost_games, lost_wins, held_games, held_wins)
        WITH teams AS (
            SELECT m.match_mode, COALESCE(m.average_badge / 10, -1) AS tier, t.team,
                   CASE t.team WHEN 0 THEN m.objectives_mask_team0 ELSE m.objectives_mask_team1 END AS mask,
                   (m.winning_team = t.team) AS won
            FROM match m
            CROSS JOIN (VALUES (0), (1)) AS t(team)
            WHERE m.start_time >= {_dayStart} AND m.start_time < {_dayEnd}
              AND m.winning_team IN (0, 1)
              AND (m.objectives_mask_team0 <> 0 OR m.objectives_mask_team1 <> 0)
        )
        SELECT @day, t.match_mode, t.tier, o.objective::smallint,
               count(*) FILTER (WHERE (t.mask >> o.objective) & 1 = 0),
               count(*) FILTER (WHERE (t.mask >> o.objective) & 1 = 0 AND t.won),
               count(*) FILTER (WHERE (t.mask >> o.objective) & 1 = 1),
               count(*) FILTER (WHERE (t.mask >> o.objective) & 1 = 1 AND t.won)
        FROM teams t
        CROSS JOIN generate_series(0, 15) AS o(objective)
        GROUP BY t.match_mode, t.tier, o.objective
        """;

    // A leaver is a player with an abandon time. Each match adds a row for each team, seen from that team.
    private const string _matchLeaverDaySql = $"""
        INSERT INTO match_leaver_day (day, mode, tier, own_leavers, enemy_leavers, games, wins, players, leaver_players)
        WITH teams AS (
            SELECT p.match_id, p.team, p.match_mode, COALESCE(p.average_badge / 10, -1) AS tier, bool_or(p.won) AS won,
                   count(*) AS players, count(*) FILTER (WHERE p.abandon_match_time_s > 0) AS leavers
            FROM match_player p
            WHERE p.start_time >= {_dayStart} AND p.start_time < {_dayEnd}
            GROUP BY p.match_id, p.team, p.match_mode, COALESCE(p.average_badge / 10, -1)
        )
        SELECT @day, a.match_mode, a.tier, LEAST(a.leavers, 3)::smallint, LEAST(b.leavers, 3)::smallint,
               count(*), count(*) FILTER (WHERE a.won), sum(a.players), sum(a.leavers)
        FROM teams a
        JOIN teams b ON b.match_id = a.match_id AND b.team <> a.team
        GROUP BY a.match_mode, a.tier, LEAST(a.leavers, 3), LEAST(b.leavers, 3)
        """;

    private readonly IDbContextFactory<DeadlockDbContext> _dbContextFactory;

    public RollupRepository(IDbContextFactory<DeadlockDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<DateOnly?> GetLatestRollupDayAsync(CancellationToken cancellationToken = default)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.MatchDays.MaxAsync(a => (DateOnly?)a.Day, cancellationToken);
    }

    public async Task<(DateOnly First, DateOnly Last)?> GetMatchDayRangeAsync(CancellationToken cancellationToken = default)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var first = await dbContext.Matches.MinAsync(a => (DateTime?)a.StartTime, cancellationToken);
        var last = await dbContext.Matches.MaxAsync(a => (DateTime?)a.StartTime, cancellationToken);
        if (first == null || last == null)
        {
            return null;
        }

        return (DateOnly.FromDateTime(first.Value.ToUniversalTime()), DateOnly.FromDateTime(last.Value.ToUniversalTime()));
    }

    public async Task RebuildDayAsync(DateOnly day, CancellationToken cancellationToken = default)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        // A day with a million players takes minutes to summarize, far longer than the default 30 seconds.
        dbContext.Database.SetCommandTimeout(TimeSpan.FromHours(2));

        // The delete and inserts commit together, so readers see either the old day or the new one, never half of it.
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        foreach (var sql in new[]
        {
            _deleteSql, _matchDaySql, _heroDaySql, _heroBanDaySql,
            _heroItemDaySql, _itemDaySql, _heroBuildShapeDaySql, _heroBuildDaySql, _heroAbilityDaySql, _heroAbilityPickDaySql,
            _heroMatchupDaySql, _heroSynergyDaySql, _heroLaneDaySql, _teamCompositionDaySql,
            _heroCurveDaySql, _teamLeadDaySql,
            _matchHourDaySql, _matchDurationDaySql, _matchObjectiveDaySql, _matchLeaverDaySql
        })
        {
            var parameter = new NpgsqlParameter("day", NpgsqlDbType.Date) { Value = day };
            await dbContext.Database.ExecuteSqlRawAsync(sql, [parameter], cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }
}
