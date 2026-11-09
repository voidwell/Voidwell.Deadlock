using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voidwell.Deadlock.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ingest_file",
                columns: table => new
                {
                    key = table.Column<string>(type: "text", nullable: false),
                    rows = table.Column<long>(type: "bigint", nullable: false),
                    ingested_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ingest_file", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "match",
                columns: table => new
                {
                    match_id = table.Column<long>(type: "bigint", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    duration_s = table.Column<int>(type: "integer", nullable: false),
                    match_mode = table.Column<short>(type: "smallint", nullable: false),
                    winning_team = table.Column<short>(type: "smallint", nullable: false),
                    average_badge_team0 = table.Column<int>(type: "integer", nullable: true),
                    average_badge_team1 = table.Column<int>(type: "integer", nullable: true),
                    average_badge = table.Column<int>(type: "integer", nullable: true),
                    banned_hero_ids = table.Column<int[]>(type: "integer[]", nullable: false),
                    objectives_mask_team0 = table.Column<int>(type: "integer", nullable: false),
                    objectives_mask_team1 = table.Column<int>(type: "integer", nullable: false),
                    is_high_skill_range_parties = table.Column<bool>(type: "boolean", nullable: true),
                    low_pri_pool = table.Column<bool>(type: "boolean", nullable: true),
                    new_player_pool = table.Column<bool>(type: "boolean", nullable: true),
                    not_scored = table.Column<bool>(type: "boolean", nullable: true),
                    game_mode_version = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_match", x => new { x.start_time, x.match_id });
                });

            migrationBuilder.CreateTable(
                name: "match_player",
                columns: table => new
                {
                    match_id = table.Column<long>(type: "bigint", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    player_slot = table.Column<int>(type: "integer", nullable: false),
                    account_id = table.Column<long>(type: "bigint", nullable: false),
                    team = table.Column<short>(type: "smallint", nullable: false),
                    hero_id = table.Column<int>(type: "integer", nullable: false),
                    won = table.Column<bool>(type: "boolean", nullable: false),
                    player_match_outcome = table.Column<short>(type: "smallint", nullable: false),
                    kills = table.Column<int>(type: "integer", nullable: false),
                    deaths = table.Column<int>(type: "integer", nullable: false),
                    assists = table.Column<int>(type: "integer", nullable: false),
                    net_worth = table.Column<int>(type: "integer", nullable: false),
                    last_hits = table.Column<int>(type: "integer", nullable: false),
                    denies = table.Column<int>(type: "integer", nullable: false),
                    ability_points = table.Column<int>(type: "integer", nullable: false),
                    player_level = table.Column<int>(type: "integer", nullable: false),
                    hero_xp = table.Column<int>(type: "integer", nullable: false),
                    mvp_rank = table.Column<int>(type: "integer", nullable: true),
                    abandon_match_time_s = table.Column<int>(type: "integer", nullable: false),
                    party = table.Column<long>(type: "bigint", nullable: true),
                    assigned_lane = table.Column<int>(type: "integer", nullable: false),
                    hero_build_id = table.Column<long>(type: "bigint", nullable: true),
                    pregame_hero_id = table.Column<int>(type: "integer", nullable: true),
                    match_mode = table.Column<short>(type: "smallint", nullable: false),
                    duration_s = table.Column<int>(type: "integer", nullable: false),
                    average_badge = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_match_player", x => new { x.start_time, x.match_id, x.player_slot });
                });

            migrationBuilder.CreateTable(
                name: "match_player_item",
                columns: table => new
                {
                    match_id = table.Column<long>(type: "bigint", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    player_slot = table.Column<int>(type: "integer", nullable: false),
                    seq = table.Column<int>(type: "integer", nullable: false),
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    upgrade_id = table.Column<long>(type: "bigint", nullable: false),
                    game_time_s = table.Column<int>(type: "integer", nullable: false),
                    sold_time_s = table.Column<int>(type: "integer", nullable: false),
                    flags = table.Column<int>(type: "integer", nullable: false),
                    imbued_ability_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_match_player_item", x => new { x.start_time, x.match_id, x.player_slot, x.seq });
                });

            migrationBuilder.CreateTable(
                name: "match_player_stat",
                columns: table => new
                {
                    match_id = table.Column<long>(type: "bigint", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    player_slot = table.Column<int>(type: "integer", nullable: false),
                    time_stamp_s = table.Column<int>(type: "integer", nullable: false),
                    net_worth = table.Column<int>(type: "integer", nullable: false),
                    gold_player = table.Column<int>(type: "integer", nullable: false),
                    gold_player_orbs = table.Column<int>(type: "integer", nullable: false),
                    gold_lane_creep_orbs = table.Column<int>(type: "integer", nullable: false),
                    gold_neutral_creep_orbs = table.Column<int>(type: "integer", nullable: false),
                    gold_boss = table.Column<int>(type: "integer", nullable: false),
                    gold_boss_orb = table.Column<int>(type: "integer", nullable: false),
                    gold_treasure = table.Column<int>(type: "integer", nullable: false),
                    gold_denied = table.Column<int>(type: "integer", nullable: false),
                    gold_death_loss = table.Column<int>(type: "integer", nullable: false),
                    gold_lane_creep = table.Column<int>(type: "integer", nullable: false),
                    gold_neutral_creep = table.Column<int>(type: "integer", nullable: false),
                    kills = table.Column<int>(type: "integer", nullable: false),
                    deaths = table.Column<int>(type: "integer", nullable: false),
                    assists = table.Column<int>(type: "integer", nullable: false),
                    creep_kills = table.Column<int>(type: "integer", nullable: false),
                    neutral_kills = table.Column<int>(type: "integer", nullable: false),
                    possible_creeps = table.Column<int>(type: "integer", nullable: false),
                    creep_damage = table.Column<int>(type: "integer", nullable: false),
                    player_damage = table.Column<int>(type: "integer", nullable: false),
                    neutral_damage = table.Column<int>(type: "integer", nullable: false),
                    boss_damage = table.Column<int>(type: "integer", nullable: false),
                    denies = table.Column<int>(type: "integer", nullable: false),
                    player_healing = table.Column<int>(type: "integer", nullable: false),
                    ability_points = table.Column<int>(type: "integer", nullable: false),
                    self_healing = table.Column<int>(type: "integer", nullable: false),
                    player_barriering = table.Column<int>(type: "integer", nullable: false),
                    teammate_healing = table.Column<int>(type: "integer", nullable: false),
                    teammate_barriering = table.Column<int>(type: "integer", nullable: false),
                    self_damage = table.Column<int>(type: "integer", nullable: false),
                    bullet_kills = table.Column<int>(type: "integer", nullable: false),
                    melee_kills = table.Column<int>(type: "integer", nullable: false),
                    ability_kills = table.Column<int>(type: "integer", nullable: false),
                    headshot_kills = table.Column<int>(type: "integer", nullable: false),
                    player_damage_taken = table.Column<int>(type: "integer", nullable: false),
                    max_health = table.Column<int>(type: "integer", nullable: false),
                    weapon_power = table.Column<int>(type: "integer", nullable: false),
                    tech_power = table.Column<int>(type: "integer", nullable: false),
                    shots_hit = table.Column<int>(type: "integer", nullable: false),
                    shots_missed = table.Column<int>(type: "integer", nullable: false),
                    damage_absorbed = table.Column<int>(type: "integer", nullable: false),
                    absorption_provided = table.Column<int>(type: "integer", nullable: false),
                    hero_bullets_hit = table.Column<int>(type: "integer", nullable: false),
                    hero_bullets_hit_crit = table.Column<int>(type: "integer", nullable: false),
                    heal_prevented = table.Column<int>(type: "integer", nullable: false),
                    heal_lost = table.Column<int>(type: "integer", nullable: false),
                    damage_mitigated = table.Column<int>(type: "integer", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_match_player_stat", x => new { x.start_time, x.match_id, x.player_slot, x.time_stamp_s });
                });

            migrationBuilder.CreateIndex(
                name: "ix_match_created_at",
                table: "match",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_match_match_id",
                table: "match",
                column: "match_id");

            migrationBuilder.CreateIndex(
                name: "ix_match_player_hero_id_start_time",
                table: "match_player",
                columns: new[] { "hero_id", "start_time" });

            migrationBuilder.CreateIndex(
                name: "ix_match_player_match_id",
                table: "match_player",
                column: "match_id");

            migrationBuilder.CreateIndex(
                name: "ix_match_player_item_item_id_start_time",
                table: "match_player_item",
                columns: new[] { "item_id", "start_time" });

            // Match data is time-series, so the match tables become TimescaleDB hypertables partitioned on start_time.
            // The tables are empty here, so no data needs migrating. Chunks are one day for the tables with many rows
            // per match. Compression is enabled but not scheduled: add the policies once the initial import has finished
            // (see scripts/enable-compression.sql), because compressing chunks while they are still being loaded is slow.
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS timescaledb;");

            migrationBuilder.Sql("SELECT create_hypertable('match', 'start_time', chunk_time_interval => INTERVAL '1 month');");
            migrationBuilder.Sql("SELECT create_hypertable('match_player', 'start_time', chunk_time_interval => INTERVAL '1 month');");
            migrationBuilder.Sql("SELECT create_hypertable('match_player_item', 'start_time', chunk_time_interval => INTERVAL '1 day');");
            migrationBuilder.Sql("SELECT create_hypertable('match_player_stat', 'start_time', chunk_time_interval => INTERVAL '1 day');");

            migrationBuilder.Sql("ALTER TABLE match SET (timescaledb.compress, timescaledb.compress_orderby = 'start_time DESC, match_id');");
            migrationBuilder.Sql("ALTER TABLE match_player SET (timescaledb.compress, timescaledb.compress_segmentby = 'hero_id', timescaledb.compress_orderby = 'start_time DESC, match_id, player_slot');");
            migrationBuilder.Sql("ALTER TABLE match_player_item SET (timescaledb.compress, timescaledb.compress_segmentby = 'item_id', timescaledb.compress_orderby = 'start_time DESC, match_id, player_slot, seq');");
            migrationBuilder.Sql("ALTER TABLE match_player_stat SET (timescaledb.compress, timescaledb.compress_orderby = 'start_time DESC, match_id, player_slot, time_stamp_s');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ingest_file");

            migrationBuilder.DropTable(
                name: "match");

            migrationBuilder.DropTable(
                name: "match_player");

            migrationBuilder.DropTable(
                name: "match_player_item");

            migrationBuilder.DropTable(
                name: "match_player_stat");
        }
    }
}
