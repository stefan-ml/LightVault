using LightVault.Domain.Interfaces;
using LightVault.Domain.Models;
using LightVault.Infrastructure.Data;
using LightVault.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace LightVault.Infrastructure.Services;

public sealed class ServiceAccountAuthService : IServiceAccountAuthService
{
    private readonly LightVaultDbContext _db;
    private readonly JwtService _jwt;
    private readonly IApiKeyHasher _hasher;

    public ServiceAccountAuthService(
        LightVaultDbContext db,
        JwtService jwt,
        IApiKeyHasher hasher)
    {
        _db = db;
        _jwt = jwt;
        _hasher = hasher;
    }

    public async Task<ServiceLoginResult?> LoginAsync(string apiKey, CancellationToken ct = default)
    {
        var accounts = await _db.ServiceAccounts
            .Where(x => x.IsActive)
            .ToListAsync(ct);

        ServiceAccount? matchedAccount = null;

        foreach (var account in accounts)
        {
            if (_hasher.Verify(apiKey, account.ApiKeyHash, account.ApiKeySalt))
            {
                matchedAccount = account;
                break;
            }
        }

        if (matchedAccount == null)
            return null;

        matchedAccount.LastUsedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var token = _jwt.CreateToken(
            matchedAccount.Id,
            matchedAccount.AppName,
            matchedAccount.Role);

        return new ServiceLoginResult
        {
            Token = token,
            AppName = matchedAccount.AppName
        };
    }
}