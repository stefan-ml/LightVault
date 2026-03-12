using LightVault.Domain.Interfaces;
using LightVault.Domain.Models;
using LightVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace LightVault.Infrastructure.Services;

public sealed class UserAuthService : IUserAuthService
{
    private readonly LightVaultDbContext _db;
    private readonly JwtService _jwt;

    public UserAuthService(LightVaultDbContext db, JwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<UserLoginResult?> LoginAsync(
        string username,
        string password,
        CancellationToken ct = default)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(x => x.Username == username && x.IsActive, ct);

        if (user == null)
            return null;

        if (!VerifyPassword(password, user.PasswordHash))
            return null;

        var token = _jwt.CreateToken(user.Id, user.Username, user.Role);

        return new UserLoginResult
        {
            Token = token,
            Username = user.Username,
            Role = user.Role
        };
    }

    private static bool VerifyPassword(string password, string hash)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes) == hash;
    }
}