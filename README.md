# Voidwell.Deadlock

[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/voidwell/voidwell.deadlock/build-test.yml?branch=main&style=for-the-badge)](https://github.com/voidwell/voidwell.deadlock/actions/workflows/build-test.yml)
[![Latest Release](https://img.shields.io/github/v/release/voidwell/voidwell.deadlock?style=for-the-badge)](https://github.com/voidwell/voidwell.deadlock/releases/latest)
[![MIT License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)

Backend API for Voidwell's Deadlock analytics. Match data lives in PostgreSQL with the TimescaleDB extension. History is bulk-loaded from the [Deadlock API data dump](https://deadlock-api.com/data-dumps) by a script, and the app keeps it current by polling the [Deadlock API](https://api.deadlock-api.com). Heroes, items and ranks are synced from the API's assets endpoints.

## Layout

| Project | Role |
|---|---|
| `Voidwell.Deadlock.Api` | ASP.NET Core host: controllers, auth, CORS, Swagger, output cache |
| `Voidwell.Deadlock.Data` | EF Core `DeadlockDbContext`, entities, configurations, repositories, migrations |
| `Voidwell.Deadlock.Analytics` | Rollup jobs that summarize the match tables by day, and the services behind the analytics endpoints |
| `Voidwell.Deadlock.Collections` | Collection layer: typed HTTP client for the API, reference-data collections and the match collection |
| `Voidwell.Deadlock.CollectionStore` | Maps API models to entities, syncs reference data weekly, and runs the `MatchUpdater` |
| `Voidwell.Deadlock.Utils` | Stateful hosted services, JSON converters |

## Match data

Normal-mode ranked and unranked matches only.

| Table | One row per | Notes |
|---|---|---|
| `match` | match | result, mode, average badges, banned heroes |
| `match_player` | player in a match | hero, result, KDA, net worth, rank; copies mode, duration and badge from the match |
| `match_player_item` | item or ability upgrade bought | buy and sell times |
| `match_player_stat` | player per stat sample | 48 cumulative stats, sampled every 180 s |
| `ingest_file` | dump file loaded | lets the import script resume |

All four match tables are TimescaleDB hypertables on `start_time`, created by the `AddMatchTables` migration (the database user needs permission to `CREATE EXTENSION timescaledb`). Nothing is deleted.

### Loading history from the data dump

`scripts/ingest/ingest_dump.py` reads the dump's `match_player` Parquet files, filters them, flattens the item and stat arrays, and copies them into the tables. It is safe to re-run: files already loaded are skipped, and rows that already exist are left alone.

```
pip install -r scripts/ingest/requirements.txt
python scripts/ingest/ingest_dump.py --dsn "postgresql://user:pass@host/voidwell.deadlock" --max-files 1   # trial run
python scripts/ingest/ingest_dump.py --dsn "postgresql://user:pass@host/voidwell.deadlock"
```

Run the EF migrations first. The dump is about 660 GB of Parquet and the import is a long run.

**Loading and parallel workers.** Each file is copied into temporary staging tables and then inserted in committed slices of about 400,000 rows (`--slice-rows`), skipping rows that already exist. The file is recorded in `ingest_file` last, so a crash simply means the file is read again. Slices matter for parallel workers (`--shard 0/2` and `--shard 1/2` in two processes): TimescaleDB locks a hypertable while an insert creates a new chunk and holds the lock until that transaction commits, so one transaction per file made workers wait for each other for hours.

**Disk use.** Loaded uncompressed, the history would be around 3 TB, so the script keeps it small as it goes:

- After each base file it compresses every chunk that ends more than `--compress-behind-days` (default 3) before that file's earliest match. Files arrive in match id order, which is close to time order, so only a few days of data are ever uncompressed. Item and stat chunks are one day long for this reason.
- It stops before loading another file once the database reaches `--stop-at-gb` (default 600). Run it again to resume.
- `--stats` prints the real compression ratio per table. A trial on one recent day measured about 13x for items and only 4.5x for stats (the cumulative stat columns compress poorly), which puts the full history at roughly 450–500 GB compressed and growth at around 1.2 GB a day. The stat table is the main cost; trimming its columns is the lever if disk gets tight.
- At the end it compresses everything older than 7 days. Then run `scripts/enable-compression.sql` so new data is compressed on a schedule.

Also keep the write-ahead log from filling the disk during the load: do not archive it, and consider `max_wal_size` of 8–16 GB and `wal_compression = on` while importing. Load the `base` files first (`--kinds base`); the `residual` and `delta` files hold rows from all periods, so loading them later writes into compressed chunks, which works but is slower.

### Keeping it current

The `MatchUpdater` hosted service runs every few minutes. It lists the matches that started in a trailing window (a match only appears once it has ended, so the window looks back past the last run), skips those already stored, and fetches and writes the rest. A run that fails is retried over the same window. It starts with the app unless `DisableMatchUpdater` is `true`, and can be controlled at runtime through `/deadlock/services`.

## Analytics

The raw match tables are far too large to query directly, so the analytics endpoints read **daily rollup tables**: `match_day`, `hero_day` and `hero_ban_day` for heroes, and `hero_item_day`, `item_day`, `hero_build_shape_day`, `hero_build_day`, `hero_ability_day` and `hero_ability_pick_day` for items, builds and abilities, `hero_matchup_day`, `hero_synergy_day`, `hero_lane_day` and `team_composition_day` for matchups and compositions, and `hero_curve_day` and `team_lead_day` for how matches develop, and `match_hour_day`, `match_duration_day`, `match_objective_day` and `match_leaver_day` for matches as a whole. Each row is one UTC day, match mode and rank tier, so any date range, patch or rank bracket is built by summing rows. `RollupUpdater` recomputes the newest days every hour. Older history is built on request, once the import has finished:

```
POST /deadlock/rollups/rebuild            (administrators; defaults to every day with matches; runs in the background)
GET  /deadlock/services/RollupUpdater/status
```

Public endpoints (cached for 10 minutes per query), all under `/deadlock`:

| Endpoint | Returns |
|---|---|
| `GET /analytics/heroes` | Win rate (with 95% interval), pick rate, ban rate, KDA, souls, damage, healing and accuracy per minute for every hero |
| `GET /analytics/heroes/tierlist` | Heroes sorted into S to D tiers by win rate |
| `GET /analytics/heroes/trends` | Win, pick and ban rate changes against an earlier period, with significance |
| `GET /analytics/heroes/{id}` | One hero's summary, daily trend and results by rank tier |
| `GET /analytics/items` | Every shop item: buy rate, win rate, how often buyers keep it, and win rate by when it was first bought |
| `GET /analytics/heroes/{id}/items` | The same for one hero's players, with each item's win rate against the hero's average |
| `GET /analytics/heroes/{id}/builds` | Win rate by how a hero's tier 3+ items split across weapon, spirit and vitality (for example `4W/2S/1V`) |
| `GET /analytics/heroes/{id}/community-builds` | Usage and win rate of the community builds players run |
| `GET /analytics/heroes/{id}/ability-order` | Which ability is maxed first, when each is unlocked, and the choice at each of the 16 ability points |
| `GET /analytics/heroes/{id}/counters` | Win rate against each enemy hero, best matchups first |
| `GET /analytics/heroes/{id}/synergies` | Win rate with each teammate hero, best pairings first |
| `GET /analytics/heroes/{id}/lane` | Win rate against each hero faced in the same lane, with the average souls lead at nine minutes |
| `GET /analytics/compositions` | Win rate by how many brawlers, assassins, marksmen, mystics and supports a team has |
| `GET /analytics/heroes/{id}/curves` | A hero's average souls, level, kills, damage, healing and power over the match, for all its players and for winners and losers |
| `GET /analytics/heroes/{id}/souls-sources` | Where a hero's souls come from over the match: kills, lane creeps, neutrals, bosses, treasure, other |
| `GET /analytics/lead` | How often the team ahead on souls wins, by sample time and size of lead (one minus that is the comeback rate); `minute` limits it to one time |
| `GET /analytics/overview` | Matches per day and per mode, match length (average, percentiles, histogram), matches by UTC hour of day, and how often each team wins |
| `GET /analytics/ranks` | How matches are spread across rank tiers |
| `GET /analytics/objectives` | Per objective: how often it is destroyed by the end of a match, and the win rate with it destroyed and standing |
| `GET /analytics/leavers` | How many players leave, and how a team does with leavers on its side or the enemy's |
| `GET /patch` | Known patches (the date is what the `patch` parameter takes) |

Query parameters, shared by the analytics endpoints:

| Parameter | Meaning |
|---|---|
| `from`, `to`, `days` | UTC days to cover. Default: the last 30 days up to the newest rolled-up day |
| `patch`, `majorPatchesOnly` | A patch release date (`2026-10-30`) or `latest`: the days after that release up to the day before the next |
| `mode` | `ranked` (default), `unranked` or `all` |
| `minTier`, `maxTier` | Rank tiers 0 to 11 (see `/rank`); ranked matches only, since unranked matches have no rank |
| `minGames` | Leave out heroes, items or builds with fewer games |
| `slot`, `itemTier` | Item endpoints only: weapon, spirit or vitality; shop tier 1 to 5 (not the rank tiers) |
| `limit` | Community builds only: how many to return, most played first |
| `compareFrom`, `compareTo`, `comparePatch` | Baseline for `trends`; defaults to the period just before |

Rates are fractions from 0 to 1. Invalid queries return 400 with the reason.

Reading the curves: the game records a player's stats at 3, 6, 9, 12 and 15 minutes and then only every 15 minutes (30, 45, 60, 75), so curves have nine points. Stats are cumulative, and later points only include matches that lasted that long, so winners and losers at 45 minutes are mostly the longer, closer games; check `samples`. Orb souls count towards the source that dropped them, and "other" is whatever the named sources do not explain (assists, breakables, team bonuses). In a typical day the team ahead at 9 minutes wins about two thirds of the time, rising to about four fifths at 30 minutes and falling again in very long matches.

Reading the objective numbers: the game records which objectives are still standing at the end of a match, not when they fell, so these show whether an objective was destroyed, not the order. A team that loses its Core or Titan almost never wins; a team that loses a tier 2 objective still wins about 40% of the time. Matches from 2024 had four lanes and later ones three, so objectives that were never destroyed in the period are left out. A leaver is a player with a recorded abandon time; in a recent day only 0.8% of players left, but a team with one lost 86% of the time.

Reading the matchup numbers: each pair result comes with `expectedWinRate`, what the two heroes' overall win rates alone would predict, and `advantage`, the difference. A hero that wins a lot will beat most heroes, so `advantage` is the figure that shows the pairing itself. Lane results use the souls lead at nine minutes, near the end of the laning phase; each lane has two players per team, so a player has two lane opponents. Heroes with no archetype in the game data (today Dynamo, Ivy, Kelvin, Paige and Rem) count as supports in compositions.

Reading the item numbers: an item is only bought late in matches that last long enough, so late items tend to show higher win rates than they earn. Use `winRateLift` together with the `timing` buckets (early under 10 minutes, mid 10 to 20, late 20 to 30, endgame 30 and over) rather than comparing items bought at different times. Players' exact final item sets are far too varied to rank (the most common set of one popular hero was under 1% of players), which is why builds are shown as slot splits and community builds.

The rollups hold about 180,000 rows for every 220,000 players a day, so rebuilding all history takes a while and adds some GB. A rebuilt day takes under a minute per 200,000 players.

Patches come from the API's feed, which only lists the latest entries, so the patch table is collected daily and builds up over time. Patch ranges only exist from the first patch it has seen.

## Configuration

Read from `appsettings.json`, `appsettings.{Environment}.json`, then environment variables (`__` for nested keys).

| Key | Required | Description |
|---|---|---|
| `ConnectionString` | Yes | Npgsql connection string |
| `Auth:Authority` / `Auth:ClientId` / `Auth:ClientSecret` | Yes | Token authority used to validate JWTs |
| `AssetsBaseUrl` | No | Defaults to `https://api.deadlock-api.com` |
| `RedisConfiguration` | No | Empty keeps the cache in memory |
| `DisableUpdater` | No | `true` disables the weekly reference-data sync |
| `DisableMatchUpdater` | No | `true` stops the match updater starting with the app |
| `MatchUpdateIntervalMinutes` | No | Minutes between match update runs (default 5) |
| `MatchUpdateOverlapMinutes` | No | How far before the last run each run looks (default 120) |
| `MatchUpdateInitialLookbackHours` | No | First run on an empty database looks this far back (default 24) |
| `MatchUpdateBatchSize` | No | Matches per API request (default 100) |
| `DisableRollupUpdater` | No | `true` stops the rollup updater starting with the app |
| `RollupIntervalMinutes` | No | Minutes between rollup runs (default 60) |
| `RollupRecomputeDays` | No | Newest days recomputed on each run (default 3) |

## Adding reference data

1. Add a model in `Collections/Models` and a collection class implementing `IStaticCollection<TModel>`; register it in `CollectionServiceExtensions`.
2. Add the entity, configuration and repository in `Data`, then `scripts/init-migrate.sh <Name>`.
3. Add a `Map` method in `CollectionEntityMaps` and register with `AddUpdaterStaticStore<TCollection, TEntity>()` in `CollectionStoreExtensions`.

Admins can force a sync with `POST /deadlock/store/update/{storeName}` (for example `HeroStore`).
