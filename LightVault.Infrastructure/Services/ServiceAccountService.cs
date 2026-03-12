using LightVault.Domain.Interfaces;
using LightVault.Domain.Models;
using LightVault.Infrastructure.Data;
using LightVault.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace LightVault.Infrastructure.Services;

public sealed class ServiceAccountService : IServiceAccountService
{
    private readonly LightVaultDbContext _context;
    private readonly IApiKeyHasher _hasher;
    private readonly IApiKeyGenerator _apiKeyGenerator;

    public ServiceAccountService(
        LightVaultDbContext context,
        IApiKeyHasher hasher,
        IApiKeyGenerator apiKeyGenerator)
    {
        _context = context;
        _hasher = hasher;
        _apiKeyGenerator = apiKeyGenerator;
    }

    public async Task<CreateServiceAccountResult> CreateAsync(
        string appName,
        string? role,
        CancellationToken ct = default)
    {
        var exists = await _context.ServiceAccounts
            .AnyAsync(x => x.AppName == appName, ct);

        if (exists)
        {
            return new CreateServiceAccountResult
            {
                Status = CreateServiceAccountStatus.AlreadyExists
            };
        }

        var apiKey = _apiKeyGenerator.Generate();
        var (hash, salt) = _hasher.Hash(apiKey);

        var entity = CreateEntity(
            appName,
            string.IsNullOrWhiteSpace(role) ? "ServiceClient" : role!,
            hash,
            salt);

        _context.ServiceAccounts.Add(entity);
        await _context.SaveChangesAsync(ct);

        return new CreateServiceAccountResult
        {
            Status = CreateServiceAccountStatus.Success,
            Id = entity.Id,
            AppName = entity.AppName,
            Role = entity.Role,
            ApiKey = apiKey
        };
    }

    public async Task<IReadOnlyList<ServiceAccountListItemResult>> GetAllAsync(
        CancellationToken ct = default)
    {
        return await _context.ServiceAccounts
            .AsNoTracking()
            .Select(x => new ServiceAccountListItemResult
            {
                Id = x.Id,
                AppName = x.AppName,
                Role = x.Role,
                IsActive = x.IsActive,
                CreatedAtUtc = x.CreatedAtUtc,
                LastUsedAtUtc = x.LastUsedAtUtc
            })
            .ToListAsync(ct);
    }

    public async Task<RevokeServiceAccountStatus> RevokeAsync(
        Guid id,
        CancellationToken ct = default)
    {
        var account = await _context.ServiceAccounts.FindAsync([id], ct);

        if (account == null)
            return RevokeServiceAccountStatus.NotFound;

        account.IsActive = false;
        await _context.SaveChangesAsync(ct);

        return RevokeServiceAccountStatus.Success;
    }

    private static ServiceAccount CreateEntity(
        string appName,
        string role,
        string apiKeyHash,
        string apiKeySalt)
    {
        return new ServiceAccount
        {
            Id = Guid.NewGuid(),
            AppName = appName,
            ApiKeyHash = apiKeyHash,
            ApiKeySalt = apiKeySalt,
            Role = role,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}