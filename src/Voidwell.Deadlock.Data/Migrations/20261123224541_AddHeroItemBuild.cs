using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voidwell.Deadlock.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHeroItemBuild : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hero_item_build",
                columns: table => new
                {
                    hero_id = table.Column<int>(type: "integer", nullable: false),
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    match_id = table.Column<long>(type: "bigint", nullable: false),
                    player_slot = table.Column<int>(type: "integer", nullable: false),
                    mode = table.Column<short>(type: "smallint", nullable: false),
                    tier = table.Column<short>(type: "smallint", nullable: false),
                    won = table.Column<bool>(type: "boolean", nullable: false),
                    duration_s = table.Column<int>(type: "integer", nullable: false),
                    item_ids = table.Column<long[]>(type: "bigint[]", nullable: false),
                    buy_times_s = table.Column<int[]>(type: "integer[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hero_item_build", x => new { x.hero_id, x.day, x.match_id, x.player_slot });
                });

            // A row per player per day is a raw-sized table, so like the match tables it is a hypertable, here partitioned
            // on day, and compressed by hero, which is how it is read. Compression is not scheduled here: add the policy
            // (scripts/enable-compression.sql) once the history has been rebuilt, so the rebuild does not write into
            // compressed chunks.
            migrationBuilder.Sql("SELECT create_hypertable('hero_item_build', 'day', chunk_time_interval => INTERVAL '7 days');");
            migrationBuilder.Sql("ALTER TABLE hero_item_build SET (timescaledb.compress, timescaledb.compress_segmentby = 'hero_id', timescaledb.compress_orderby = 'day DESC, match_id, player_slot');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hero_item_build");
        }
    }
}
