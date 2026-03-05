using LightVault.API.DTOs;
using LightVault.Domain.Interfaces;
using LightVault.Infrastructure.Data;
using LightVault.Infrastructure.Entities;
using LightVault.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LightVault.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly LightVaultDbContext _db;
    private readonly JwtService _jwt;
    private readonly IApiKeyHasher _hasher;

    public AuthController(
        LightVaultDbContext db,
        JwtService jwt,
        IApiKeyHasher hasher)
    {
        _db = db;
        _jwt = jwt;
        _hasher = hasher;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x =>
            x.Username == req.Username && x.IsActive);

        if (user == null) return Unauthorized();

        if (!VerifyPassword(req.Password, user.PasswordHash))
            return Unauthorized();

        var token = _jwt.CreateToken(user.Id, user.Username, user.Role);

        return Ok(new { token, user.Username, user.Role });
    }

    private static bool VerifyPassword(string password, string hash)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes) == hash;
    }

    [HttpPost("service-login")]
    [AllowAnonymous]
    public async Task<IActionResult> ServiceLogin(ServiceLoginRequest request)
    {
        var accounts = await _db.ServiceAccounts
            .Where(x => x.IsActive)
            .ToListAsync();

        ServiceAccount? matchedAccount = null;

        foreach (var account in accounts)
        {
            if (_hasher.Verify(request.ApiKey, account.ApiKeyHash, account.ApiKeySalt))
            {
                matchedAccount = account;
                break;
            }
        }

        if (matchedAccount == null)
            return Unauthorized("Invalid API key.");

        matchedAccount.LastUsedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var token = _jwt.CreateToken(
            matchedAccount.Id,
            matchedAccount.AppName,
            matchedAccount.Role);

        return Ok(new
        {
            token,
            AppName = matchedAccount.AppName
        });
    }
}

public class LoginRequest
{
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
}
