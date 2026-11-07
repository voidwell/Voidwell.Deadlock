# Voidwell.Deadlock

Backend API for Voidwell's Deadlock data. It pulls static game data (heroes, items) from the [Deadlock assets API](https://api.deadlock-api.com), stores it in PostgreSQL, caches it in Redis (optional) and serves it over an ASP.NET Core HTTP API.

## Layout

| Project | Role |
|---|---|
| `Voidwell.Deadlock.Api` | ASP.NET Core host: controllers, auth, CORS, Swagger, output cache |
| `Voidwell.Deadlock.Data` | EF Core `DeadlockDbContext`, entities, configurations, repositories, migrations |
| `Voidwell.Deadlock.Collections` | Collection layer: typed HTTP client for the assets API and one collection class per resource |
| `Voidwell.Deadlock.CollectionStore` | Maps collection models to entities, schedules weekly syncs into the database, cached read stores |
| `Voidwell.Deadlock.Utils` | Stateful hosted services, JSON converters |

## Configuration

Read from `appsettings.json`, `appsettings.{Environment}.json`, then environment variables (`__` for nested keys).

| Key | Required | Description |
|---|---|---|
| `ConnectionString` | Yes | Npgsql connection string |
| `Auth:Authority` / `Auth:ClientId` / `Auth:ClientSecret` | Yes | Token authority used to validate JWTs |
| `AssetsBaseUrl` | No | Defaults to `https://api.deadlock-api.com` |
| `RedisConfiguration` | No | Empty keeps the cache in memory |
| `DisableUpdater` | No | `true` disables the scheduled collection sync |

## Adding a collection

1. Add a model in `Collections/Models` and a collection class implementing `IStaticCollection<TModel>`; register it in `CollectionServiceExtensions`.
2. Add the entity, configuration and repository in `Data`, then `scripts/init-migrate.sh <Name>`.
3. Add a `Map` method in `CollectionEntityMaps` and register with `AddUpdaterStaticStore<TCollection, TEntity>()` in `CollectionStoreExtensions`.

Admins can force a sync with `POST /deadlock/store/update/{storeName}` (for example `HeroStore`).
