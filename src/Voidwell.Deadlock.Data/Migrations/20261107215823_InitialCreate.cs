using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voidwell.Deadlock.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hero",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    class_name = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    hero_type = table.Column<string>(type: "text", nullable: true),
                    development_state = table.Column<string>(type: "text", nullable: true),
                    is_playable = table.Column<bool>(type: "boolean", nullable: false),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    icon_url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hero", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "item",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    class_name = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<string>(type: "text", nullable: true),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    slot_type = table.Column<string>(type: "text", nullable: true),
                    tier = table.Column<int>(type: "integer", nullable: true),
                    cost = table.Column<int>(type: "integer", nullable: true),
                    is_shopable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rank",
                columns: table => new
                {
                    tier = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    color = table.Column<string>(type: "text", nullable: true),
                    image_url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rank", x => x.tier);
                });

            migrationBuilder.CreateTable(
                name: "updater_scheduler",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    last_update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_updater_scheduler", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hero");

            migrationBuilder.DropTable(
                name: "item");

            migrationBuilder.DropTable(
                name: "rank");

            migrationBuilder.DropTable(
                name: "updater_scheduler");
        }
    }
}
