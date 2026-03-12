using LightVault.Domain.Interfaces;
using LightVault.Domain.Models;
using LightVault.Infrastructure.Data;
using LightVault.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace LightVault.Infrastructure.Services;

public sealed class SecretService : ISecretService
{
    private readonly LightVaultDbContext _db;
    private readonly IEncryptionService _encryptionService;
    private readonly AuditService _auditService;
    private readonly IUserContext _userContext;

    public SecretService(
        LightVaultDbContext db,
        IEncryptionService encryptionService,
        AuditService auditService,
        IUserContext userContext)
    {
        _db = db;
        _encryptionService = encryptionService;
        _auditService = auditService;
        _userContext = userContext;
    }

    public async Task<IReadOnlyList<SecretListItemResult>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _db.Secrets
            .AsNoTracking()
            .Select(x => new SecretListItemResult
            {
                Id = x.Id,
                Name = x.Name,
                CurrentVersion = x.CurrentVersion
            })
            .ToListAsync(ct);

        await _auditService.LogAsync(_userContext.ActorName, "LIST_SECRETS", null, null, ct);

        return list;
    }

    public async Task<SecretDetailsResult?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var sec = await _db.Secrets
            .Include(x => x.Versions)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (sec == null)
            return null;

        await _auditService.LogAsync(_userContext.ActorName, "READ_SECRET", id, null, ct);

        var latest = sec.Versions
            .OrderByDescending(v => v.Version)
            .First();

        var value = _encryptionService.DecryptToString(
            latest.Ciphertext,
            latest.Nonce,
            latest.Tag,
            sec.Name);

        return new SecretDetailsResult
        {
            Id = sec.Id,
            Name = sec.Name,
            CurrentVersion = sec.CurrentVersion,
            Value = value,
            ExpiresAt = latest.ExpiresAt,
            CanModifySecret = CanModifySecret(sec),
            Versions = sec.Versions
                .OrderByDescending(v => v.Version)
                .Select(v => new SecretVersionItemResult
                {
                    Version = v.Version,
                    CreatedAt = v.CreatedAt
                })
                .ToList()
        };
    }

    public async Task<Guid> CreateAsync(string name, string value, CancellationToken ct = default)
    {
        var existing = await _db.Secrets
            .Include(s => s.Versions)
            .FirstOrDefaultAsync(s => s.Name == name, ct);

        var (cipher, nonce, tag) = _encryptionService.EncryptString(value, name);
        var actor = _userContext.ActorName;

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
            await _auditService.LogAsync(
                actor,
                "UPDATED_SECRET_VERSION",
                existing.Id,
                $"name={existing.Name};version={newVersion}",
                ct);

            return existing.Id;
        }

        var secret = new SecretEntity
        {
            Name = name,
            CurrentVersion = 1,
            CreatedByUserId = _userContext.UserId
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
        await _auditService.LogAsync(
            actor,
            "CREATED_SECRET",
            secret.Id,
            $"name={secret.Name};version=1",
            ct);

        return secret.Id;
    }

    public async Task<SecretVersionDetailsResult?> GetByVersionAsync(Guid id, int version, CancellationToken ct = default)
    {
        var sec = await _db.Secrets
            .Include(x => x.Versions)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (sec == null)
            return null;

        var ver = sec.Versions.FirstOrDefault(v => v.Version == version);
        if (ver == null)
            return null;

        await _auditService.LogAsync(_userContext.ActorName, "READ_SECRET_VERSION", id, $"version={version}", ct);

        var value = _encryptionService.DecryptToString(ver.Ciphertext, ver.Nonce, ver.Tag, sec.Name);

        return new SecretVersionDetailsResult
        {
            Id = sec.Id,
            Name = sec.Name,
            Version = ver.Version,
            Value = value,
            CreatedAt = ver.CreatedAt,
            ExpiresAt = ver.ExpiresAt
        };
    }

    public async Task<RotateSecretResult?> RotateAsync(Guid id, string newValue, CancellationToken ct = default)
    {
        var sec = await _db.Secrets
            .Include(s => s.Versions)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (sec == null)
            return null;

        if (!CanModifySecret(sec))
        {
            await _auditService.LogAsync(_userContext.ActorName, "ROTATE_SECRET_FORBIDDEN", sec.Id, null, ct);
            throw new UnauthorizedAccessException("User cannot modify this secret.");
        }

        var newVersion = sec.CurrentVersion + 1;
        var (cipher, nonce, tag) = _encryptionService.EncryptString(newValue, sec.Name);

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
        await _auditService.LogAsync(
            _userContext.ActorName,
            "ROTATED_SECRET",
            sec.Id,
            $"name={sec.Name};newVersion={newVersion}",
            ct);

        return new RotateSecretResult
        {
            Id = sec.Id,
            Name = sec.Name,
            NewVersion = newVersion
        };
    }

    public async Task<SecretByNameResult?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var sec = await _db.Secrets
            .Include(x => x.Versions)
            .FirstOrDefaultAsync(x => x.Name == name, ct);

        if (sec == null)
            return null;

        await _auditService.LogAsync(_userContext.ActorName, "READ_SECRET_BY_NAME", sec.Id, $"name={name}", ct);

        var latest = sec.Versions
            .OrderByDescending(v => v.Version)
            .First();

        var value = _encryptionService.DecryptToString(
            latest.Ciphertext,
            latest.Nonce,
            latest.Tag,
            sec.Name);

        return new SecretByNameResult
        {
            Id = sec.Id,
            Name = sec.Name,
            CurrentVersion = sec.CurrentVersion,
            Value = value,
            ExpiresAt = latest.ExpiresAt
        };
    }

    private bool CanModifySecret(SecretEntity sec)
    {
        if (_userContext.IsInRole("Admin"))
            return true;

        if (_userContext.IsInRole("Developer"))
            return sec.CreatedByUserId == _userContext.UserId;

        return false;
    }
}