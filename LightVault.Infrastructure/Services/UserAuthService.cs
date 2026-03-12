using LightVault.Domain.Interfaces;
using LightVault.Domain.Models;
using LightVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LightVault.Infrastructure.Services;

public sealed class UserAuthService : IUserAuthService
{
    private readonly LightVaultDbContext _db;
    private readonly JwtService _jwt;
    private readonly IPasswordHasher _passwordHasher;

    public UserAuthService(
        LightVaultDbContext db,
        JwtService jwt,
        IPasswordHasher passwordHasher)
    {
        _db = db;
        _jwt = jwt;
        _passwordHasher = passwordHasher;
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

        if (!_passwordHasher.Verify(password, user.PasswordHash))
            return null;

        var token = _jwt.CreateToken(user.Id, user.Username, user.Role);

        return new UserLoginResult
        {
            Token = token,
            Username = user.Username,
            Role = user.Role
        };
    }
}