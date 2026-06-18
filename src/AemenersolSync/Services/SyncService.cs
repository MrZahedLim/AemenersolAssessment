using AemenersolSync.Data;
using AemenersolSync.Dtos;
using AemenersolSync.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AemenersolSync.Services;

public class SyncService
{
    private readonly AemenersolApiClient _api;
    private readonly AppDbContext _db;
    private readonly ILogger<SyncService> _logger;

    public SyncService(AemenersolApiClient api, AppDbContext db, ILogger<SyncService> logger)
    {
        _api = api;
        _db = db;
        _logger = logger;
    }

    public async Task RunAsync(bool useDummy = false, CancellationToken ct = default)
    {
        var token = await _api.LoginAsync(ct);

        var platforms = useDummy
            ? await _api.GetPlatformWellDummyAsync(token, ct)
            : await _api.GetPlatformWellActualAsync(token, ct);

        if (platforms.Count == 0)
        {
            _logger.LogWarning("No platforms returned from the API. Nothing to sync.");
            return;
        }

        // Platform dulu sebab Well rujuk PlatformId (FK)
        await UpsertPlatformsAsync(platforms, ct);
        await UpsertWellsAsync(platforms, ct);

        var changes = await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Sync complete. {Changes} row change(s) saved.", changes);
    }

    private async Task UpsertPlatformsAsync(List<PlatformDto> dtos, CancellationToken ct)
    {
        var ids = dtos.Select(d => d.Id).ToList();

        // load yang dah wujud sekali gus, elak query satu-satu
        var existing = await _db.Platforms
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        var inserted = 0;
        var updated = 0;

        foreach (var dto in dtos)
        {
            if (existing.TryGetValue(dto.Id, out var entity))
            {
                MapPlatform(dto, entity);
                updated++;
            }
            else
            {
                entity = new Platform { Id = dto.Id };
                MapPlatform(dto, entity);
                _db.Platforms.Add(entity);
                inserted++;
            }
        }

        _logger.LogInformation("Platforms -> {Inserted} insert(s), {Updated} update(s).", inserted, updated);
    }

    private async Task UpsertWellsAsync(List<PlatformDto> dtos, CancellationToken ct)
    {
        var wellDtos = dtos.SelectMany(p => p.Well).ToList();
        if (wellDtos.Count == 0)
            return;

        var ids = wellDtos.Select(w => w.Id).ToList();
        var existing = await _db.Wells
            .Where(w => ids.Contains(w.Id))
            .ToDictionaryAsync(w => w.Id, ct);

        var inserted = 0;
        var updated = 0;

        foreach (var dto in wellDtos)
        {
            if (existing.TryGetValue(dto.Id, out var entity))
            {
                MapWell(dto, entity);
                updated++;
            }
            else
            {
                entity = new Well { Id = dto.Id };
                MapWell(dto, entity);
                _db.Wells.Add(entity);
                inserted++;
            }
        }

        _logger.LogInformation("Wells -> {Inserted} insert(s), {Updated} update(s).", inserted, updated);
    }

    // kalau tarikh null (cth Dummy), kekal nilai sedia ada supaya column tak rosak
    private static void MapPlatform(PlatformDto dto, Platform entity)
    {
        entity.UniqueName = dto.UniqueName ?? string.Empty;
        entity.Latitude = dto.Latitude;
        entity.Longitude = dto.Longitude;
        entity.CreatedAt = dto.CreatedAt ?? entity.CreatedAt;
        entity.UpdatedAt = dto.UpdatedAt ?? entity.UpdatedAt;
    }

    private static void MapWell(WellDto dto, Well entity)
    {
        entity.PlatformId = dto.PlatformId;
        entity.UniqueName = dto.UniqueName ?? string.Empty;
        entity.Latitude = dto.Latitude;
        entity.Longitude = dto.Longitude;
        entity.CreatedAt = dto.CreatedAt ?? entity.CreatedAt;
        entity.UpdatedAt = dto.UpdatedAt ?? entity.UpdatedAt;
    }
}
