using LightVault.API.DTOs;
using LightVault.Domain.Interfaces;
using LightVault.Infrastructure.Data;
using LightVault.Infrastructure.Entities;
using LightVault.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LightVault.API.Controllers;

[ApiController]
[Route("api/secrets")]
[Authorize]
public class SecretsController : ControllerBase
{
    private readonly LightVaultDbContext _db;
    private readonly IEncryptionService _enc;
    private readonly AuditService _audit;

    public SecretsController(LightVaultDbContext db, IEncryptionService enc, AuditService audit)
    {
        _db = db;
        _enc = enc;
        _audit = audit;
    }

    private string GetActor()
    {
        return User.FindFirst("username")?.Value
            ?? User.FindFirst(ClaimTypes.Name)?.Value
            ?? "Unknown";
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var list = await _db.Secrets
            .AsNoTracking()
            .Select(x => new { x.Id, x.Name, x.CurrentVersion })
            .ToListAsync(ct);

        await _audit.LogAsync(GetActor(), "LIST_SECRETS", null, null, ct);

        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct = default)
    {
        var sec = await _db.Secrets
            .Include(x => x.Versions)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (sec == null) return NotFound();

        await _audit.LogAsync(GetActor(), "READ_SECRET", id, null, ct);

        var latest = sec.Versions.OrderByDescending(v => v.Version).First();
        var value = _enc.DecryptToString(latest.Ciphertext, latest.Nonce, latest.Tag, sec.Name);

        return Ok(new
        {
            sec.Id,
            sec.Name,
            sec.CurrentVersion,
            Value = value,
            Versions = sec.Versions
                .Select(v => new { v.Version, v.CreatedAt })
                .OrderByDescending(v => v.Version),
            ExpiresAt = latest.ExpiresAt,
            CanModifySecret = CanModifySecret(sec)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSecretRequest req, CancellationToken ct = default)
    {
        var existing = await _db.Secrets
            .Include(s => s.Versions)
            .FirstOrDefaultAsync(s => s.Name == req.Name, ct);

        var (cipher, nonce, tag) = _enc.EncryptString(req.Value, req.Name);
        var actor = GetActor();
        var actorUserId = GetActorUserId();

        if (existing != null)
        {
            var newVersion = existing.CurrentVersion + 1;

            _db.SecretVersions.Add(new SecretVersionEntity
            {
                SecretId = existing.Id,
                Version = newVersion,
                Ciphertext = cipher,
                Nonce = nonce,
                Tag = tag,
                ExpiresAt = DateTime.UtcNow.AddDays(90)
            });

            existing.CurrentVersion = newVersion;

            await _db.SaveChangesAsync(ct);
            await _audit.LogAsync(actor, "UPDATED_SECRET_VERSION", existing.Id, $"name={existing.Name};version={newVersion}", ct);

            return Ok(existing.Id);
        }

        var secret = new SecretEntity
        {
            Name = req.Name,
            CurrentVersion = 1,
            CreatedByUserId = actorUserId
        };

        _db.Secrets.Add(secret);

        _db.SecretVersions.Add(new SecretVersionEntity
        {
            Secret = secret,
            Version = 1,
            Ciphertext = cipher,
            Nonce = nonce,
            Tag = tag,
            ExpiresAt = DateTime.UtcNow.AddDays(90)
        });

        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(actor, "CREATED_SECRET", secret.Id, $"name={secret.Name};version=1", ct);

        return Ok(secret.Id);
    }

    [HttpGet("{id:guid}/versions/{version:int}")]
    public async Task<IActionResult> GetByVersion(Guid id, int version, CancellationToken ct = default)
    {
        var sec = await _db.Secrets
            .Include(x => x.Versions)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (sec == null)
            return NotFound();

        var ver = sec.Versions.FirstOrDefault(v => v.Version == version);
        if (ver == null)
            return NotFound($"Version {version} does not exist.");

        await _audit.LogAsync(GetActor(), "READ_SECRET_VERSION", id, $"version={version}", ct);

        var value = _enc.DecryptToString(ver.Ciphertext, ver.Nonce, ver.Tag, sec.Name);

        return Ok(new
        {
            Id = sec.Id,
            Name = sec.Name,
            Version = ver.Version,
            Value = value,
            CreatedAt = ver.CreatedAt,
            ExpiresAt = ver.ExpiresAt
        });
    }

    [HttpPost("{id:guid}/rotate")]
    public async Task<IActionResult> Rotate(Guid id, RotateSecretRequest req, CancellationToken ct = default)
    {
        var sec = await _db.Secrets
            .Include(s => s.Versions)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (sec == null)
            return NotFound();

        if (!CanModifySecret(sec))
        {
            await _audit.LogAsync(GetActor(), "ROTATE_SECRET_FORBIDDEN", sec.Id, null, ct);
            return Forbid();
        }

        var actor = GetActor();
        var newVersion = sec.CurrentVersion + 1;

        var (cipher, nonce, tag) = _enc.EncryptString(req.NewValue, sec.Name);

        _db.SecretVersions.Add(new SecretVersionEntity
        {
            SecretId = sec.Id,
            Version = newVersion,
            Ciphertext = cipher,
            Nonce = nonce,
            Tag = tag,
            ExpiresAt = DateTime.UtcNow.AddDays(90)
        });

        sec.CurrentVersion = newVersion;

        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(actor, "ROTATED_SECRET", sec.Id, $"name={sec.Name};newVersion={newVersion}", ct);

        return Ok(new
        {
            sec.Id,
            sec.Name,
            NewVersion = newVersion
        });
    }

    [Authorize(Roles = "Admin,Developer,ServiceClient")]
    [HttpGet("by-name/{name}")]
    public async Task<IActionResult> GetByName(string name, CancellationToken ct = default)
    {
        var sec = await _db.Secrets
            .Include(x => x.Versions)
            .FirstOrDefaultAsync(x => x.Name == name, ct);

        if (sec == null)
            return NotFound();

        await _audit.LogAsync(GetActor(), "READ_SECRET_BY_NAME", sec.Id, $"name={name}", ct);

        var latest = sec.Versions
            .OrderByDescending(v => v.Version)
            .First();

        var value = _enc.DecryptToString(
            latest.Ciphertext,
            latest.Nonce,
            latest.Tag,
            sec.Name
        );

        return Ok(new
        {
            sec.Id,
            sec.Name,
            sec.CurrentVersion,
            Value = value,
            ExpiresAt = latest.ExpiresAt
        });
    }

    private async Task RevokeSecretVersionAsync(int versionId, CancellationToken ct = default)
    {
        var version = await _db.SecretVersions.FindAsync([versionId], ct);
        if (version == null)
            return;

        version.IsRevoked = true;
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(GetActor(), "REVOKED_SECRET_VERSION", version.SecretId, $"versionId={versionId}", ct);
    }

    private Guid GetActorUserId()
    {
        var id =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        return Guid.TryParse(id, out var guid)
            ? guid
            : throw new InvalidOperationException("UserId claim missing or invalid.");
    }

    private bool IsAdmin() => User.IsInRole("Admin");
    private bool IsDeveloper() => User.IsInRole("Developer");
    private bool CanModifySecret(SecretEntity sec)
    {
        if (IsAdmin()) return true; 
        if (IsDeveloper()) return sec.CreatedByUserId == GetActorUserId();
        return false;
    }

}
