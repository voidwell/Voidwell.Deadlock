using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voidwell.Deadlock.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddItemBuildAbilityRollups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hero_ability_day",
                columns: table => new
                {
                    hero_id = table.Column<int>(type: "integer", nullable: false),
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    mode = table.Column<short>(type: "smallint", nullable: false),
                    tier = table.Column<short>(type: "smallint", nullable: false),
                    ability_id = table.Column<long>(type: "bigint", nullable: false),
                    games = table.Column<int>(type: "integer", nullable: false),
                    wins = table.Column<int>(type: "integer", nullable: false),
                    maxed_games = table.Column<int>(type: "integer", nullable: false),
                    maxed_wins = table.Column<int>(type: "integer", nullable: false),
                    max_first_games = table.Column<int>(type: "integer", nullable: false),
                    max_first_wins = table.Column<int>(type: "integer", nullable: false),
                    unlock_time_sum_s = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hero_ability_day", x => new { x.hero_id, x.day, x.mode, x.tier, x.ability_id });
                });

            migrationBuilder.CreateTable(
                name: "hero_ability_pick_day",
                columns: table => new
                {
                    hero_id = table.Column<int>(type: "integer", nullable: false),
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    mode = table.Column<short>(type: "smallint", nullable: false),
                    tier = table.Column<short>(type: "smallint", nullable: false),
                    pick_index = table.Column<short>(type: "smallint", nullable: false),
                    ability_id = table.Column<long>(type: "bigint", nullable: false),
                    games = table.Column<int>(type: "integer", nullable: false),
                    wins = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hero_ability_pick_day", x => new { x.hero_id, x.day, x.mode, x.tier, x.pick_index, x.ability_id });
                });

            migrationBuilder.CreateTable(
                name: "hero_build_day",
                columns: table => new
                {
                    hero_id = table.Column<int>(type: "integer", nullable: false),
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    mode = table.Column<short>(type: "smallint", nullable: false),
                    tier = table.Column<short>(type: "smallint", nullable: false),
                    build_id = table.Column<long>(type: "bigint", nullable: false),
                    games = table.Column<int>(type: "integer", nullable: false),
                    wins = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hero_build_day", x => new { x.hero_id, x.day, x.mode, x.tier, x.build_id });
                });

            migrationBuilder.CreateTable(
                name: "hero_build_shape_day",
                columns: table => new
                {
                    hero_id = table.Column<int>(type: "integer", nullable: false),
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    mode = table.Column<short>(type: "smallint", nullable: false),
                    tier = table.Column<short>(type: "smallint", nullable: false),
                    weapon_items = table.Column<short>(type: "smallint", nullable: false),
                    spirit_items = table.Column<short>(type: "smallint", nullable: false),
                    vitality_items = table.Column<short>(type: "smallint", nullable: false),
                    games = table.Column<int>(type: "integer", nullable: false),
                    wins = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hero_build_shape_day", x => new { x.hero_id, x.day, x.mode, x.tier, x.weapon_items, x.spirit_items, x.vitality_items });
                });

            migrationBuilder.CreateTable(
                name: "hero_item_day",
                columns: table => new
                {
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    hero_id = table.Column<int>(type: "integer", nullable: false),
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    mode = table.Column<short>(type: "smallint", nullable: false),
                    tier = table.Column<short>(type: "smallint", nullable: false),
                    games = table.Column<int>(type: "integer", nullable: false),
                    wins = table.Column<int>(type: "integer", nullable: false),
                    held_games = table.Column<int>(type: "integer", nullable: false),
                    held_wins = table.Column<int>(type: "integer", nullable: false),
                    buy_time_sum_s = table.Column<long>(type: "bigint", nullable: false),
                    early_games = table.Column<int>(type: "integer", nullable: false),
                    early_wins = table.Column<int>(type: "integer", nullable: false),
                    mid_games = table.Column<int>(type: "integer", nullable: false),
                    mid_wins = table.Column<int>(type: "integer", nullable: false),
                    late_games = table.Column<int>(type: "integer", nullable: false),
                    late_wins = table.Column<int>(type: "integer", nullable: false),
                    endgame_games = table.Column<int>(type: "integer", nullable: false),
                    endgame_wins = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hero_item_day", x => new { x.hero_id, x.day, x.mode, x.tier, x.item_id });
                });

            migrationBuilder.CreateTable(
                name: "item_day",
                columns: table => new
                {
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    mode = table.Column<short>(type: "smallint", nullable: false),
                    tier = table.Column<short>(type: "smallint", nullable: false),
                    games = table.Column<int>(type: "integer", nullable: false),
                    wins = table.Column<int>(type: "integer", nullable: false),
                    held_games = table.Column<int>(type: "integer", nullable: false),
                    held_wins = table.Column<int>(type: "integer", nullable: false),
                    buy_time_sum_s = table.Column<long>(type: "bigint", nullable: false),
                    early_games = table.Column<int>(type: "integer", nullable: false),
                    early_wins = table.Column<int>(type: "integer", nullable: false),
                    mid_games = table.Column<int>(type: "integer", nullable: false),
                    mid_wins = table.Column<int>(type: "integer", nullable: false),
                    late_games = table.Column<int>(type: "integer", nullable: false),
                    late_wins = table.Column<int>(type: "integer", nullable: false),
                    endgame_games = table.Column<int>(type: "integer", nullable: false),
                    endgame_wins = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_day", x => new { x.day, x.mode, x.tier, x.item_id });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hero_ability_day");

            migrationBuilder.DropTable(
                name: "hero_ability_pick_day");

            migrationBuilder.DropTable(
                name: "hero_build_day");

            migrationBuilder.DropTable(
                name: "hero_build_shape_day");

            migrationBuilder.DropTable(
                name: "hero_item_day");

            migrationBuilder.DropTable(
                name: "item_day");
        }
    }
}
