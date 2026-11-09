using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voidwell.Deadlock.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCurveRollups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hero_curve_day",
                columns: table => new
                {
                    hero_id = table.Column<int>(type: "integer", nullable: false),
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    mode = table.Column<short>(type: "smallint", nullable: false),
                    tier = table.Column<short>(type: "smallint", nullable: false),
                    won = table.Column<bool>(type: "boolean", nullable: false),
                    time_stamp_s = table.Column<int>(type: "integer", nullable: false),
                    samples = table.Column<int>(type: "integer", nullable: false),
                    net_worth = table.Column<long>(type: "bigint", nullable: false),
                    level = table.Column<long>(type: "bigint", nullable: false),
                    kills = table.Column<long>(type: "bigint", nullable: false),
                    deaths = table.Column<long>(type: "bigint", nullable: false),
                    assists = table.Column<long>(type: "bigint", nullable: false),
                    player_damage = table.Column<long>(type: "bigint", nullable: false),
                    player_damage_taken = table.Column<long>(type: "bigint", nullable: false),
                    player_healing = table.Column<long>(type: "bigint", nullable: false),
                    weapon_power = table.Column<long>(type: "bigint", nullable: false),
                    tech_power = table.Column<long>(type: "bigint", nullable: false),
                    max_health = table.Column<long>(type: "bigint", nullable: false),
                    kill_souls = table.Column<long>(type: "bigint", nullable: false),
                    lane_souls = table.Column<long>(type: "bigint", nullable: false),
                    neutral_souls = table.Column<long>(type: "bigint", nullable: false),
                    boss_souls = table.Column<long>(type: "bigint", nullable: false),
                    treasure_souls = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hero_curve_day", x => new { x.hero_id, x.day, x.mode, x.tier, x.won, x.time_stamp_s });
                });

            migrationBuilder.CreateTable(
                name: "team_lead_day",
                columns: table => new
                {
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    mode = table.Column<short>(type: "smallint", nullable: false),
                    tier = table.Column<short>(type: "smallint", nullable: false),
                    time_stamp_s = table.Column<int>(type: "integer", nullable: false),
                    lead_bucket = table.Column<short>(type: "smallint", nullable: false),
                    games = table.Column<int>(type: "integer", nullable: false),
                    leader_wins = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_team_lead_day", x => new { x.day, x.mode, x.tier, x.time_stamp_s, x.lead_bucket });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hero_curve_day");

            migrationBuilder.DropTable(
                name: "team_lead_day");
        }
    }
}
