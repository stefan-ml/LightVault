using System.Data;
using System.Security.Cryptography;
using System.Text;
using LightVault.Infrastructure.Data;
using LightVault.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LightVault.Infrastructure.Services;

public class AuditService
{
    private const string GenesisHash = "GENESIS";

    private readonly LightVaultDbContext _db;
    private readonly byte[] _signingKey;
    private readonly string _anchorPath;

    public AuditService(LightVaultDbContext db, IConfiguration configuration)
    {
        _db = db;

        var key = configuration["AuditSigningKey"];
        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException("Missing environment variable: AuditSigningKey");

        _signingKey = Encoding.UTF8.GetBytes(key);

        _anchorPath = configuration["AuditAnchorPath"]
        ?? throw new InvalidOperationException("Missing environment variable: AuditAnchorPath");
    }

    /// <summary>
    /// Append-only audit log entry (tamper-evident hash chain).
    /// Uses HMAC-SHA256 with AuditSigningKey so DB-only attackers cannot rehash the chain.
    /// </summary>
    public async Task<AuditEntryEntity> LogAsync(
        string actor,
        string action,
        Guid? secretId = null,
        string? details = null,
        CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        var lastEntry = await _db.AuditEntries
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(ct);

        var prevHash = lastEntry?.Hash ?? GenesisHash;

        var timestampUtc = DateTime.UtcNow;

        var entry = new AuditEntryEntity
        {
            Actor = actor,
            Action = action,
            SecretId = secretId,
            Details = details,
            TimestampUtc = timestampUtc,
            PrevHash = prevHash,
            Hash = string.Empty 
        };

        var canonicalPayload = BuildCanonicalPayload(entry);

        entry.Hash = ComputeHmacSha256Hex(_signingKey, prevHash + "|" + canonicalPayload);

        _db.AuditEntries.Add(entry);
        await _db.SaveChangesAsync(ct);

        await tx.CommitAsync(ct);

        await AuditAnchor.WriteAsync(_anchorPath, entry.Id, entry.Hash, ct);

        return entry;
    }

    /// <summary>
    /// Verifies the audit hash chain.
    /// Detects:
    /// - manual UPDATE of any historical record (hash mismatch)
    /// - DELETE/reorder in the middle (PrevHash mismatch)
    /// Limit (without anchor): trimming tail entries can’t be proven.
    /// </summary>
    public async Task<VerifyResult> VerifyChainAsync(CancellationToken ct = default)
    {
        var entries = await _db.AuditEntries
            .OrderBy(x => x.Id)
            .AsNoTracking()
            .ToListAsync(ct);

        if (entries.Count == 0) return VerifyResult.Ok();

        for (int i = 0; i < entries.Count; i++)
        {
            var current = entries[i];
            var expectedPrev = i == 0 ? GenesisHash : entries[i - 1].Hash;

            if (!FixedTimeEquals(current.PrevHash, expectedPrev))
            {
                return VerifyResult.Fail(brokenAtId: current.Id, reason: "PrevHash mismatch (deleted/reordered entry or tampered PrevHash).");
            }

            if (expectedPrev == GenesisHash) { continue; }
            var canonicalPayload = BuildCanonicalPayload(current);
            var expectedHash = ComputeHmacSha256Hex(_signingKey, expectedPrev + "|" + canonicalPayload);

            if (!FixedTimeEquals(current.Hash, expectedHash))
            {
                return VerifyResult.Fail(brokenAtId: current.Id, reason: "Hash mismatch (entry modified).");
            }
        }
        var last = entries[^1];
        var anchor = await AuditAnchor.ReadAsync(_anchorPath, ct);
        if (anchor is not null)
        {
            if (last.Id != anchor.LastId ||
                !FixedTimeEquals(last.Hash, anchor.LastHash))
            {
                return VerifyResult.Fail(
                    brokenAtId: last.Id,
                    reason: "Latest audit entry does not match anchor file. Possible deletion or truncation of tail entries."
                );
            }
        }
        return VerifyResult.Ok(lastId: entries[^1].Id, lastHash: entries[^1].Hash);
    }

    /// <summary>
    /// Deterministic “canonical” representation of the entry content used for hashing.
    /// </summary>
    private static string BuildCanonicalPayload(AuditEntryEntity e)
    {
        var ts = DateTime.SpecifyKind(e.TimestampUtc, DateTimeKind.Utc).ToString("O");

        return
            $"actor={e.Actor};" +
            $"action={e.Action};" +
            $"secretId={(e.SecretId?.ToString() ?? "null")};" +
            $"details={(e.Details ?? "null")};" +
            $"timestampUtc={ts}";
    }

    private static string ComputeHmacSha256Hex(byte[] key, string input)
    {
        using var hmac = new HMACSHA256(key);
        var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes); // uppercase hex
    }

    private static bool FixedTimeEquals(string? a, string? b)
    {
        a ??= "";
        b ??= "";
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(a),
            Encoding.UTF8.GetBytes(b)
        );
    }

    public sealed record VerifyResult(
        bool IsValid,
        long? BrokenAtId,
        string Reason,
        long? LastId,
        string? LastHash)
    {
        public static VerifyResult Ok(long? lastId = null, string? lastHash = null)
            => new(true, null, "OK", lastId, lastHash);

        public static VerifyResult Fail(long brokenAtId, string reason)
            => new(false, brokenAtId, reason, null, null);
    }
}
