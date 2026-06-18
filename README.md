# Aemenersol Assessment

.NET 8 console app that syncs Platform & Well data from a REST API into SQL
Server using EF Core (Code First), plus one SQL query for Part 2.

## Part 1 — .NET App

What it does:

1. Login to the Web API (`POST /api/Account/Login`) and get the bearer token.
2. Use the token to call `GetPlatformWellActual`.
3. Save into `Platform` and `Well` tables. If the Id exists -> update, else
   insert (upsert by id).
4. Doesn't break when the API returns a different shape (missing/new keys). Test
   it with `GetPlatformWellDummy`.

### Structure

```
src/AemenersolSync/
├── Models/        Platform.cs, Well.cs        (Code First entities)
├── Dtos/          PlatformDto, WellDto, LoginRequest
├── Data/          AppDbContext.cs
├── Services/      AemenersolApiClient, SyncService, ApiSettings
├── Migrations/    InitialCreate
├── Program.cs
└── appsettings.json / appsettings.Development.json
```

### Handling different data shapes

- New key added -> `System.Text.Json` just ignores keys with no property.
- Original key missing -> date properties use `DateTime?`, so it's null instead
  of an error. The Dummy endpoint swaps `createdAt`/`updatedAt` for `lastUpdate`;
  the original keys become null and mapping falls back to the existing value.

Only keys from the original dataset are mapped, as required.

### How to run

Needs .NET 8 SDK + SQL Server.

LocalDB (Windows) — `appsettings.json` already points to LocalDB:

```
cd src/AemenersolSync
dotnet run               # GetPlatformWellActual
dotnet run -- --dummy    # GetPlatformWellDummy
```

Migrations auto-apply on startup (`db.Database.MigrateAsync()`), so the database
and tables are created on the first run.

Docker SQL (I developed on Mac, LocalDB is Windows-only):

```
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Test1234" \
  -p 1433:1433 --name aemenersol-sql -d mcr.microsoft.com/mssql/server:2022-latest

cd src/AemenersolSync
DOTNET_ENVIRONMENT=Development dotnet run
```

`appsettings.Development.json` overrides the connection string to the container.

### Tested

- 1st run: 10 platforms + 15 wells inserted.
- 2nd run: all updated (upsert by id works).
- `--dummy` run: different shape, no crash.

## Part 2 — SQL Query

`sql/LastUpdatedWellPerPlatform.sql` returns the last updated well per platform.
It uses `ROW_NUMBER() OVER (PARTITION BY PlatformId ORDER BY UpdatedAt DESC)` to
rank each platform's wells and keep the top one.

## Time

Around 3-4 hours. Most of it went into exploring the API first (comparing Actual
vs Dummy to figure out how to handle the different shape) and making sure the
upsert actually updates instead of always inserting. Setting up SQL Server via
Docker (LocalDB isn't on Mac) took a bit too.
