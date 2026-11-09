using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voidwell.Deadlock.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchupRollups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hero_lane_day",
                columns: table => new
                {
                    hero_id = table.Column<int>(type: "integer", nullable: false),
                    enemy_hero_id = table.Column<int>(type: "integer", nullable: false),
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    mode = table.Column<short>(type: "smallint", nullable: false),
                    tier = table.Column<short>(type: "smallint", nullable: false),
                    games = table.Column<int>(type: "integer", nullable: false),
                    wins = table.Column<int>(type: "integer", nullable: false),
                    lead_games = table.Column<int>(type: "integer", nullable: false),
                    lead_sum = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hero_lane_day", x => new { x.hero_id, x.day, x.mode, x.tier, x.enemy_hero_id });
                });

            migrationBuilder.CreateTable(
                name: "hero_matchup_day",
                columns: table => new
                {
                    hero_id = table.Column<int>(type: "integer", nullable: false),
                    enemy_hero_id = table.Column<int>(type: "integer", nullable: false),
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    mode = table.Column<short>(type: "smallint", nullable: false),
                    tier = table.Column<short>(type: "smallint", nullable: false),
                    games = table.Column<int>(type: "integer", nullable: false),
                    wins = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hero_matchup_day", x => new { x.hero_id, x.day, x.mode, x.tier, x.enemy_hero_id });
                });

            migrationBuilder.CreateTable(
                name: "hero_synergy_day",
                columns: table => new
                {
                    hero_id = table.Column<int>(type: "integer", nullable: false),
                    ally_hero_id = table.Column<int>(type: "integer", nullable: false),
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    mode = table.Column<short>(type: "smallint", nullable: false),
                    tier = table.Column<short>(type: "smallint", nullable: false),
                    games = table.Column<int>(type: "integer", nullable: false),
                    wins = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hero_synergy_day", x => new { x.hero_id, x.day, x.mode, x.tier, x.ally_hero_id });
                });

            migrationBuilder.CreateTable(
                name: "team_composition_day",
                columns: table => new
                {
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    mode = table.Column<short>(type: "smallint", nullable: false),
                    tier = table.Column<short>(type: "smallint", nullable: false),
                    brawlers = table.Column<short>(type: "smallint", nullable: false),
                    assassins = table.Column<short>(type: "smallint", nullable: false),
                    marksmen = table.Column<short>(type: "smallint", nullable: false),
                    mystics = table.Column<short>(type: "smallint", nullable: false),
                    supports = table.Column<short>(type: "smallint", nullable: false),
                    games = table.Column<int>(type: "integer", nullable: false),
                    wins = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_team_composition_day", x => new { x.day, x.mode, x.tier, x.brawlers, x.assassins, x.marksmen, x.mystics, x.supports });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hero_lane_day");

            migrationBuilder.DropTable(
                name: "hero_matchup_day");

            migrationBuilder.DropTable(
                name: "hero_synergy_day");

            migrationBuilder.DropTable(
                name: "team_composition_day");
        }
    }
}
