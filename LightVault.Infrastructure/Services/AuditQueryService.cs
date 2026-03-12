using LightVault.Domain.Interfaces;
using LightVault.Domain.Models;
using LightVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LightVault.Infrastructure.Services;

public sealed class AuditQueryService : IAuditQueryService
{
    private const int MaxItems = 500;
    private readonly LightVaultDbContext _db;

    public AuditQueryService(LightVaultDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<AuditEntryResult>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.AuditEntries
            .AsNoTracking()
            .OrderByDescending(x => x.TimestampUtc)
            .Take(MaxItems)
            .Select(x => new AuditEntryResult
            {
                Id = x.Id,
                Actor = x.Actor,
                Action = x.Action,
                Details = x.Details,
                TimestampUtc = x.TimestampUtc,
                Hash = x.Hash,
                PreviousHash = x.PrevHash
            })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AuditEntryResult>> FilterAsync(
        string? user,
        string? action,
        DateTime? from,
        DateTime? to,
        CancellationToken ct = default)
    {
        var query = _db.AuditEntries.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(user))
            query = query.Where(x => x.Actor.Contains(user));

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(x => x.Action.Contains(action));

        if (from.HasValue)
            query = query.Where(x => x.TimestampUtc >= DateTime.SpecifyKind(from.Value, DateTimeKind.Utc));

        if (to.HasValue)
            query = query.Where(x => x.TimestampUtc <= DateTime.SpecifyKind(to.Value, DateTimeKind.Utc));

        return await query
            .OrderByDescending(x => x.TimestampUtc)
            .Take(MaxItems)
            .Select(x => new AuditEntryResult
            {
                Id = x.Id,
                Actor = x.Actor,
                Action = x.Action,
                Details = x.Details,
                TimestampUtc = x.TimestampUtc,
                Hash = x.Hash,
                PreviousHash = x.PrevHash
            })
            .ToListAsync(ct);
    }
}