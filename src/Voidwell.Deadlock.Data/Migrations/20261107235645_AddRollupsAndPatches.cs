using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voidwell.Deadlock.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRollupsAndPatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hero_ban_day",
                columns: table => new
                {
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    mode = table.Column<short>(type: "smallint", nullable: false),
                    tier = table.Column<short>(type: "smallint", nullable: false),
                    hero_id = table.Column<int>(type: "integer", nullable: false),
                    bans = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hero_ban_day", x => new { x.day, x.mode, x.tier, x.hero_id });
                });

            migrationBuilder.CreateTable(
                name: "hero_day",
                columns: table => new
                {
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    mode = table.Column<short>(type: "smallint", nullable: false),
                    tier = table.Column<short>(type: "smallint", nullable: false),
                    hero_id = table.Column<int>(type: "integer", nullable: false),
                    games = table.Column<int>(type: "integer", nullable: false),
                    wins = table.Column<int>(type: "integer", nullable: false),
                    kills = table.Column<long>(type: "bigint", nullable: false),
                    deaths = table.Column<long>(type: "bigint", nullable: false),
                    assists = table.Column<long>(type: "bigint", nullable: false),
                    net_worth = table.Column<long>(type: "bigint", nullable: false),
                    last_hits = table.Column<long>(type: "bigint", nullable: false),
                    denies = table.Column<long>(type: "bigint", nullable: false),
                    duration_sum_s = table.Column<long>(type: "bigint", nullable: false),
                    stat_games = table.Column<int>(type: "integer", nullable: false),
                    player_damage = table.Column<long>(type: "bigint", nullable: false),
                    player_damage_taken = table.Column<long>(type: "bigint", nullable: false),
                    boss_damage = table.Column<long>(type: "bigint", nullable: false),
                    player_healing = table.Column<long>(type: "bigint", nullable: false),
                    shots_hit = table.Column<long>(type: "bigint", nullable: false),
                    shots_missed = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hero_day", x => new { x.day, x.mode, x.tier, x.hero_id });
                });

            migrationBuilder.CreateTable(
                name: "match_day",
                columns: table => new
                {
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    mode = table.Column<short>(type: "smallint", nullable: false),
                    tier = table.Column<short>(type: "smallint", nullable: false),
                    matches = table.Column<int>(type: "integer", nullable: false),
                    matches_with_bans = table.Column<int>(type: "integer", nullable: false),
                    team0wins = table.Column<int>(type: "integer", nullable: false),
                    duration_sum_s = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_match_day", x => new { x.day, x.mode, x.tier });
                });

            migrationBuilder.CreateTable(
                name: "patch",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    source = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: true),
                    link = table.Column<string>(type: "text", nullable: true),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_big_day = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_patch", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_hero_day_hero_id_day",
                table: "hero_day",
                columns: new[] { "hero_id", "day" });

            migrationBuilder.CreateIndex(
                name: "ix_patch_published_at",
                table: "patch",
                column: "published_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hero_ban_day");

            migrationBuilder.DropTable(
                name: "hero_day");

            migrationBuilder.DropTable(
                name: "match_day");

            migrationBuilder.DropTable(
                name: "patch");
        }
    }
}
