using LightVault.API.DTOs;
using LightVault.Domain.Interfaces;
using LightVault.Infrastructure.Data;
using LightVault.Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

[ApiController]
[Route("api/service-accounts")]
public class ServiceAccountsController : ControllerBase
{
    private readonly LightVaultDbContext _context;
    private readonly IApiKeyHasher _hasher;

    public ServiceAccountsController(
        LightVaultDbContext context,
        IApiKeyHasher hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateServiceAccountRequest request)
    {
        if (await _context.ServiceAccounts
            .AnyAsync(x => x.AppName == request.AppName))
        {
            return BadRequest("Service account already exists.");
        }

        var apiKeyBytes = RandomNumberGenerator.GetBytes(32);
        var apiKey = Convert.ToBase64String(apiKeyBytes);

        var (hash, salt) = _hasher.Hash(apiKey);

        var entity = new ServiceAccount
        {
            Id = Guid.NewGuid(),
            AppName = request.AppName,
            ApiKeyHash = hash,
            ApiKeySalt = salt,
            Role = request.Role ?? "ServiceClient",
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.ServiceAccounts.Add(entity);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            entity.Id,
            entity.AppName,
            entity.Role,
            ApiKey = apiKey 
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _context.ServiceAccounts
            .Select(x => new
            {
                x.Id,
                x.AppName,
                x.Role,
                x.IsActive,
                x.CreatedAtUtc,
                x.LastUsedAtUtc
            })
            .ToListAsync();

        return Ok(list);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Revoke(Guid id)
    {
        var account = await _context.ServiceAccounts.FindAsync(id);
        if (account == null)
            return NotFound();

        account.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}