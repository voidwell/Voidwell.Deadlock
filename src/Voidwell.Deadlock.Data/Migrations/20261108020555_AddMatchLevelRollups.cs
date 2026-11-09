using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voidwell.Deadlock.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchLevelRollups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "match_duration_day",
                columns: table => new
                {
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    mode = table.Column<short>(type: "smallint", nullable: false),
                    tier = table.Column<short>(type: "smallint", nullable: false),
                    minute = table.Column<short>(type: "smallint", nullable: false),
                    matches = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_match_duration_day", x => new { x.day, x.mode, x.tier, x.minute });
                });

            migrationBuilder.CreateTable(
                name: "match_hour_day",
                columns: table => new
                {
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    mode = table.Column<short>(type: "smallint", nullable: false),
                    tier = table.Column<short>(type: "smallint", nullable: false),
                    hour = table.Column<short>(type: "smallint", nullable: false),
                    matches = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_match_hour_day", x => new { x.day, x.mode, x.tier, x.hour });
                });

            migrationBuilder.CreateTable(
                name: "match_leaver_day",
                columns: table => new
                {
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    mode = table.Column<short>(type: "smallint", nullable: false),
                    tier = table.Column<short>(type: "smallint", nullable: false),
                    own_leavers = table.Column<short>(type: "smallint", nullable: false),
                    enemy_leavers = table.Column<short>(type: "smallint", nullable: false),
                    games = table.Column<int>(type: "integer", nullable: false),
                    wins = table.Column<int>(type: "integer", nullable: false),
                    players = table.Column<long>(type: "bigint", nullable: false),
                    leaver_players = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_match_leaver_day", x => new { x.day, x.mode, x.tier, x.own_leavers, x.enemy_leavers });
                });

            migrationBuilder.CreateTable(
                name: "match_objective_day",
                columns: table => new
                {
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    mode = table.Column<short>(type: "smallint", nullable: false),
                    tier = table.Column<short>(type: "smallint", nullable: false),
                    objective = table.Column<short>(type: "smallint", nullable: false),
                    lost_games = table.Column<int>(type: "integer", nullable: false),
                    lost_wins = table.Column<int>(type: "integer", nullable: false),
                    held_games = table.Column<int>(type: "integer", nullable: false),
                    held_wins = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_match_objective_day", x => new { x.day, x.mode, x.tier, x.objective });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "match_duration_day");

            migrationBuilder.DropTable(
                name: "match_hour_day");

            migrationBuilder.DropTable(
                name: "match_leaver_day");

            migrationBuilder.DropTable(
                name: "match_objective_day");
        }
    }
}
