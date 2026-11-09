#!/usr/bin/env python3
"""Loads the Deadlock API data dump (match_player Parquet files) into the app's TimescaleDB tables.

For each file in the dump manifest that has not been loaded yet, the script
  1. downloads the file,
  2. uses DuckDB to keep Normal-mode ranked and unranked matches and flatten them into four CSV files
     (match, match_player, match_player_item, match_player_stat),
  3. copies them into Postgres inside one transaction, skipping rows that already exist, and records the file in
     ingest_file so a re-run skips it.

Run it after the EF migrations have created the tables. The app keeps the data current afterwards; re-running the script
is safe and fills any matches the app missed.

Usage:
  python ingest_dump.py --dsn "postgresql://user:pass@host/voidwell.deadlock"
  python ingest_dump.py --extract-only path/to/file.parquet     # no database needed; prints row counts
"""
import argparse
import json
import os
import shutil
import sys
import tempfile
import time
import urllib.request
from datetime import datetime, timedelta, timezone

import duckdb

MANIFEST_URL = "https://data.deadlock-api.com/v1/manifest.json"

# Same filter the app uses when it collects matches.
MATCH_FILTER = "game_mode = 'Normal' AND match_mode IN ('Ranked', 'Unranked')"

INT_MAX = 2147483647

STATS = """net_worth gold_player gold_player_orbs gold_lane_creep_orbs gold_neutral_creep_orbs gold_boss gold_boss_orb
gold_treasure gold_denied gold_death_loss gold_lane_creep gold_neutral_creep kills deaths assists creep_kills neutral_kills
possible_creeps creep_damage player_damage neutral_damage boss_damage denies player_healing ability_points self_healing
player_barriering teammate_healing teammate_barriering self_damage bullet_kills melee_kills ability_kills headshot_kills
player_damage_taken max_health weapon_power tech_power shots_hit shots_missed damage_absorbed absorption_provided
hero_bullets_hit hero_bullets_hit_crit heal_prevented heal_lost damage_mitigated level""".split()


def to_int(col):
    """The dump stores unsigned 32-bit values; the tables use signed 32-bit, so out-of-range values are capped."""
    return f"CASE WHEN {col} IS NULL THEN NULL ELSE CAST(LEAST({col}, {INT_MAX}) AS INTEGER) END"


def team_code(col):
    return f"CASE {col} WHEN 'Team0' THEN 0 WHEN 'Team1' THEN 1 ELSE 16 END"


def match_mode_code(col):
    return f"CASE {col} WHEN 'Unranked' THEN 1 WHEN 'Ranked' THEN 4 ELSE 0 END"


def outcome_code(col):
    return (f"CASE {col} WHEN 'Win' THEN 1 WHEN 'Loss' THEN 2 WHEN 'Penalized' THEN 3 "
            f"WHEN 'PenalizedParty' THEN 4 WHEN 'NotScored' THEN 5 ELSE 0 END")


# Each table: (target table, column list in COPY order, SELECT producing those columns from the `src` view).
TABLES = {
    "match": (
        ["match_id", "start_time", "duration_s", "match_mode", "winning_team", "average_badge_team0", "average_badge_team1",
         "average_badge", "banned_hero_ids", "objectives_mask_team0", "objectives_mask_team1", "is_high_skill_range_parties",
         "low_pri_pool", "new_player_pool", "not_scored", "game_mode_version"],
        f"""
        SELECT
            CAST(match_id AS BIGINT),
            any_value(start_time),
            {to_int('any_value(duration_s)')},
            {match_mode_code('any_value(match_mode)')},
            {team_code('any_value(winning_team)')},
            {to_int('any_value(average_badge_team0)')},
            {to_int('any_value(average_badge_team1)')},
            {to_int('any_value(average_badge)')},
            '{{' || coalesce(list_aggr(any_value(banned_hero_ids), 'string_agg', ','), '') || '}}',
            {to_int('any_value(objectives_mask_team0)')},
            {to_int('any_value(objectives_mask_team1)')},
            any_value(is_high_skill_range_parties),
            any_value(low_pri_pool),
            any_value(new_player_pool),
            any_value(not_scored),
            {to_int('any_value(game_mode_version)')}
        FROM src GROUP BY match_id
        """,
    ),
    "match_player": (
        ["match_id", "start_time", "player_slot", "account_id", "team", "hero_id", "won", "player_match_outcome", "kills",
         "deaths", "assists", "net_worth", "last_hits", "denies", "ability_points", "player_level", "hero_xp", "mvp_rank",
         "abandon_match_time_s", "party", "assigned_lane", "hero_build_id", "pregame_hero_id", "match_mode", "duration_s",
         "average_badge"],
        f"""
        SELECT
            CAST(match_id AS BIGINT), start_time, {to_int('player_slot')}, CAST(account_id AS BIGINT),
            {team_code('team')}, {to_int('hero_id')}, won, {outcome_code('player_match_outcome')},
            {to_int('kills')}, {to_int('deaths')}, {to_int('assists')}, {to_int('net_worth')}, {to_int('last_hits')},
            {to_int('denies')}, {to_int('ability_points')}, {to_int('player_level')}, {to_int('hero_xp')}, {to_int('mvp_rank')},
            {to_int('abandon_match_time_s')}, CAST(party AS BIGINT), {to_int('assigned_lane')}, CAST(hero_build_id AS BIGINT),
            {to_int('pregame_hero_id')}, {match_mode_code('match_mode')}, {to_int('duration_s')}, {to_int('average_badge')}
        FROM src
        """,
    ),
    "match_player_item": (
        ["match_id", "start_time", "player_slot", "seq", "item_id", "upgrade_id", "game_time_s", "sold_time_s", "flags",
         "imbued_ability_id"],
        f"""
        SELECT match_id, start_time, player_slot, seq, item_id, upgrade_id, {to_int('game_time_s')}, {to_int('sold_time_s')},
               {to_int('flags')}, imbued_ability_id
        FROM (
            SELECT
                CAST(match_id AS BIGINT) AS match_id, start_time, {to_int('player_slot')} AS player_slot,
                unnest(range(len("items.item_id"))) AS seq,
                CAST(unnest("items.item_id") AS BIGINT) AS item_id,
                CAST(unnest("items.upgrade_id") AS BIGINT) AS upgrade_id,
                unnest("items.game_time_s") AS game_time_s,
                unnest("items.sold_time_s") AS sold_time_s,
                unnest("items.flags") AS flags,
                CAST(unnest("items.imbued_ability_id") AS BIGINT) AS imbued_ability_id
            FROM src
        )
        """,
    ),
    "match_player_stat": (
        ["match_id", "start_time", "player_slot", "time_stamp_s"] + STATS,
        f"""
        SELECT match_id, start_time, player_slot, {to_int('time_stamp_s')},
               {', '.join(to_int(s) for s in STATS)}
        FROM (
            SELECT
                CAST(match_id AS BIGINT) AS match_id, start_time, {to_int('player_slot')} AS player_slot,
                unnest("stats.time_stamp_s") AS time_stamp_s,
                {', '.join(f'unnest("stats.{s}") AS {s}' for s in STATS)}
            FROM src
        )
        """,
    ),
}


def log(message):
    print(f"[{time.strftime('%H:%M:%S')}] {message}", flush=True)


def open_url(url):
    # The dump host rejects Python's default User-Agent with a 403, so identify the client explicitly.
    return urllib.request.urlopen(urllib.request.Request(url, headers={"User-Agent": "Voidwell.Deadlock-ingest"}))


def download(url, path):
    with open_url(url) as response, open(path, "wb") as out:
        shutil.copyfileobj(response, out, length=1024 * 1024)


def extract(parquet_path, out_dir, threads, memory_limit):
    """Writes one CSV per table for the file and returns {table: row count}."""
    con = duckdb.connect()
    con.execute("SET TimeZone = 'UTC'")
    con.execute(f"SET threads = {int(threads)}")
    con.execute(f"SET memory_limit = '{memory_limit}'")

    # Row order does not matter here, and not preserving it lets DuckDB stream large files instead of holding them in memory.
    con.execute("SET preserve_insertion_order = false")
    con.execute(f"SET temp_directory = '{out_dir.replace(chr(92), '/')}'")

    path = parquet_path.replace("\\", "/")
    con.execute(f"CREATE VIEW src AS SELECT * FROM read_parquet('{path}') WHERE {MATCH_FILTER}")

    counts = {}
    for table, (columns, select) in TABLES.items():
        csv_path = os.path.join(out_dir, f"{table}.csv").replace("\\", "/")
        con.execute(f"COPY ({select}) TO '{csv_path}' (FORMAT csv, HEADER false)")
        counts[table] = con.execute(f"SELECT count(*) FROM read_csv('{csv_path}', header=false, all_varchar=true)").fetchone()[0]

    min_start = con.execute("SELECT min(start_time) FROM src").fetchone()[0]

    con.close()
    return counts, min_start


def load(dsn, key, out_dir, counts, slice_rows):
    """Loads a file's CSVs into the tables in small committed slices.

    Timescale locks the hypertable while an insert creates a chunk, and holds the lock until the transaction commits. One
    transaction per file lasts hours, so parallel workers would wait for each other's chunk creation. Committing a slice at
    a time keeps the lock to seconds. This is safe to repeat: rows that exist are skipped, and the file is only recorded as
    loaded after everything else, so a crash just means the file is read again.
    """
    import psycopg  # imported late so --extract-only works without it

    with psycopg.connect(dsn, autocommit=True) as conn:
        # Durability on commit is not needed (a file can be loaded again), and it makes the load noticeably faster.
        conn.execute("SET synchronous_commit = off")

        for table, (columns, _) in TABLES.items():
            if counts[table] == 0:
                continue

            column_list = ", ".join(columns)
            staging = f"stg_{table}"
            conn.execute(f"DROP TABLE IF EXISTS {staging}")
            conn.execute(f"CREATE TEMP TABLE {staging} (LIKE {table} INCLUDING DEFAULTS)")

            with conn.cursor() as cur:
                with open(os.path.join(out_dir, f"{table}.csv"), "rb") as csv_file:
                    with cur.copy(f"COPY {staging} ({column_list}) FROM STDIN WITH (FORMAT csv)") as copy:
                        while chunk := csv_file.read(1024 * 1024):
                            copy.write(chunk)

            # Slice by match id, which follows time, so each slice touches only a few chunks. The slices are sized by rows.
            low, high = conn.execute(f"SELECT min(match_id), max(match_id) FROM {staging}").fetchone()
            slices = max(1, -(-counts[table] // slice_rows))
            step = max(1, -(-(high - low + 1) // slices))

            added = 0
            for start in range(low, high + 1, step):
                cur = conn.execute(
                    f"INSERT INTO {table} ({column_list}) SELECT {column_list} FROM {staging} "
                    f"WHERE match_id >= %s AND match_id < %s ON CONFLICT DO NOTHING",
                    (start, start + step),
                )
                added += cur.rowcount

            conn.execute(f"DROP TABLE {staging}")
            log(f"  {table}: {counts[table]:,} rows read, {added:,} new ({slices} slices)")

        conn.execute(
            "INSERT INTO ingest_file (key, rows, ingested_at) VALUES (%s, %s, now()) "
            "ON CONFLICT (key) DO UPDATE SET rows = EXCLUDED.rows, ingested_at = EXCLUDED.ingested_at",
            (key, counts["match_player"]),
        )


def already_loaded(dsn):
    import psycopg

    with psycopg.connect(dsn) as conn:
        return {row[0] for row in conn.execute("SELECT key FROM ingest_file")}


def database_size_gb(dsn):
    import psycopg

    with psycopg.connect(dsn) as conn:
        return conn.execute("SELECT pg_database_size(current_database())").fetchone()[0] / 1e9


def compress_older_than(dsn, cutoff):
    """Compresses every chunk of the match tables that ends before the cutoff. Chunks already compressed are skipped."""
    import psycopg

    # Compression runs outside a transaction so each chunk is committed (and its disk space reclaimed) as it finishes.
    with psycopg.connect(dsn, autocommit=True) as conn:
        for table in TABLES:
            chunks = [r[0] for r in conn.execute("SELECT show_chunks(%s::regclass, older_than => %s)", (table, cutoff)).fetchall()]
            for chunk in chunks:
                conn.execute("SELECT compress_chunk(%s::regclass, if_not_compressed => true)", (chunk,))


def print_stats(dsn):
    """Prints how well each table has compressed so far, so the size estimate can be checked early."""
    import psycopg

    with psycopg.connect(dsn) as conn:
        print(f"{'table':<20}{'uncompressed GB':>18}{'compressed GB':>16}{'ratio':>8}")
        for table in TABLES:
            row = conn.execute(
                "SELECT sum(before_compression_total_bytes), sum(after_compression_total_bytes) FROM chunk_compression_stats(%s::regclass)",
                (table,),
            ).fetchone()
            before, after = float(row[0] or 0) / 1e9, float(row[1] or 0) / 1e9
            print(f"{table:<20}{before:>18.1f}{after:>16.1f}{(before / after if after else 0):>8.1f}")
        print(f"database size: {database_size_gb(dsn):.1f} GB")


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--dsn", default=os.environ.get("DEADLOCK_PG_DSN"), help="Postgres connection string (or set DEADLOCK_PG_DSN)")
    parser.add_argument("--manifest-url", default=MANIFEST_URL)
    parser.add_argument("--kinds", default="base,residual,delta", help="Manifest file kinds to load, in this order")
    parser.add_argument("--max-files", type=int, help="Stop after this many files (useful for a trial run)")
    parser.add_argument("--work-dir", help="Scratch directory for downloads and CSVs (needs room for the largest file)")
    parser.add_argument("--threads", type=int, default=4)
    parser.add_argument("--memory-limit", default="12GB", help="Memory DuckDB may use per worker; the largest files need several GB")
    parser.add_argument("--compress-behind-days", type=int, default=3,
                        help="After each base file, compress chunks that end this many days before the file's earliest match "
                             "(0 turns this off). Keeps the uncompressed data to a few days instead of the whole history.")
    parser.add_argument("--stop-at-gb", type=float, default=600,
                        help="Stop before loading another file once the database is this large (0 turns this off)")
    parser.add_argument("--shard", metavar="I/N",
                        help="Load only every Nth pending file, starting at the Ith (for example 0/2 and 1/2 in two processes)")
    parser.add_argument("--finalize", action="store_true",
                        help="Compress all chunks older than 7 days, print statistics and exit. Run once after all workers finish.")
    parser.add_argument("--slice-rows", type=int, default=400_000,
                        help="Rows inserted per committed slice. Smaller slices hold the chunk-creation lock for less time")
    parser.add_argument("--stats", action="store_true", help="Print compression statistics and exit")
    parser.add_argument("--extract-only", metavar="PARQUET", help="Convert one local Parquet file and print row counts; no database")
    args = parser.parse_args()

    work_root = args.work_dir or tempfile.gettempdir()

    if args.extract_only:
        out_dir = tempfile.mkdtemp(prefix="deadlock-extract-", dir=work_root)
        counts, min_start = extract(args.extract_only, out_dir, args.threads, args.memory_limit)
        for table, count in counts.items():
            print(f"{table}: {count:,} rows -> {os.path.join(out_dir, table + '.csv')}")
        print(f"earliest match: {min_start}")
        return 0

    if not args.dsn:
        parser.error("--dsn or DEADLOCK_PG_DSN is required")

    if args.stats:
        print_stats(args.dsn)
        return 0

    if args.finalize:
        log("Compressing chunks older than 7 days")
        compress_older_than(args.dsn, datetime.now(timezone.utc) - timedelta(days=7))
        print_stats(args.dsn)
        log("Done. Now run scripts/enable-compression.sql so new data is compressed on a schedule.")
        return 0

    log("Reading manifest")
    with open_url(args.manifest_url) as response:
        manifest = json.load(response)

    base_url = manifest["public_url"].rstrip("/")
    kinds = args.kinds.split(",")
    files = [f for f in manifest["tables"]["match_player"]["files"] if f["kind"] in kinds]
    files.sort(key=lambda f: (kinds.index(f["kind"]), f.get("partition", 0), f.get("built_at", "")))

    done = already_loaded(args.dsn)
    todo = [f for f in files if f["key"] not in done]
    log(f"{len(files)} match_player files in the manifest, {len(done)} already loaded, {len(todo)} to load")

    if args.shard:
        shard_index, shard_count = (int(x) for x in args.shard.split("/"))
        todo = [f for position, f in enumerate(todo) if position % shard_count == shard_index]
        log(f"Shard {args.shard}: {len(todo)} files")

    if args.max_files:
        todo = todo[: args.max_files]

    for index, file in enumerate(todo, start=1):
        key = file["key"]

        if args.stop_at_gb:
            size = database_size_gb(args.dsn)
            if size >= args.stop_at_gb:
                log(f"Database is {size:,.0f} GB, at or over --stop-at-gb {args.stop_at_gb:,.0f}. Stopping; run again to resume.")
                print_stats(args.dsn)
                return 2

        log(f"[{index}/{len(todo)}] {key} ({file['rows']:,} rows, {file['bytes'] / 1e6:,.0f} MB)")

        out_dir = tempfile.mkdtemp(prefix="deadlock-ingest-", dir=work_root)
        try:
            parquet_path = os.path.join(out_dir, "source.parquet")
            download(f"{base_url}/{key}", parquet_path)
            counts, min_start = extract(parquet_path, out_dir, args.threads, args.memory_limit)
            os.remove(parquet_path)
            load(args.dsn, key, out_dir, counts, args.slice_rows)
        finally:
            shutil.rmtree(out_dir, ignore_errors=True)

        # Base files arrive in match id order, which is close to time order, so chunks well behind the file being loaded
        # are finished. Residual and delta files hold rows from all periods and are not used to move the cutoff.
        if args.compress_behind_days and file["kind"] == "base" and min_start is not None:
            cutoff = min_start - timedelta(days=args.compress_behind_days)
            compress_older_than(args.dsn, cutoff)

    if not args.max_files and not args.shard:
        # The whole history has been loaded, so compress everything the scheduled policy would (older than 7 days).
        log("Compressing remaining chunks older than 7 days")
        compress_older_than(args.dsn, datetime.now(timezone.utc) - timedelta(days=7))
        print_stats(args.dsn)
        log("Done. Now run scripts/enable-compression.sql so new data is compressed on a schedule.")
    else:
        log("Done" + (" (run --finalize once every shard has finished)" if args.shard else ""))

    return 0


if __name__ == "__main__":
    sys.exit(main())
