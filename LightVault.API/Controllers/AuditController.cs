using LightVault.Infrastructure.Data;
using LightVault.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LightVault.API.Controllers;

[ApiController]
[Route("api/audit")]
[Authorize(Roles = "Admin,Auditor")]
public class AuditController : ControllerBase
{
    private readonly LightVaultDbContext _db;
    private readonly AuditService _auditService;

    public AuditController(LightVaultDbContext db, AuditService auditService)
    {
        _db = db;
        _auditService = auditService;
    }

    // GET /api/audit
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var list = await _db.AuditEntries
            .AsNoTracking()
            .OrderByDescending(x => x.TimestampUtc)
            .Take(500)
            .ToListAsync(ct);

        return Ok(list);
    }

    // GET /api/audit/filter?user=...&action=...&from=...&to=...
    [HttpGet("filter")]
    public async Task<IActionResult> Filter(
        [FromQuery] string? user,
        [FromQuery] string? action,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
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

        var result = await query
            .OrderByDescending(x => x.TimestampUtc)
            .Take(500)
            .ToListAsync(ct);

        return Ok(result);
    }

    // GET /api/audit/verify
    [HttpGet("verify")]
    public async Task<IActionResult> Verify(CancellationToken ct = default)
    {
        var result = await _auditService.VerifyChainAsync(ct);
        return Ok(result);
    }
}
