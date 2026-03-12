using LightVault.Domain.Interfaces;
using LightVault.Domain.Models;
using LightVault.Infrastructure.Data;
using LightVault.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace LightVault.Infrastructure.Services;

public sealed class UserManagementService : IUserManagementService
{
    private readonly LightVaultDbContext _db;
    private readonly IPasswordHasher _passwordHasher;

    public UserManagementService(
        LightVaultDbContext db,
        IPasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDetailsResult?> GetAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserDetailsResult
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role,
                IsActive = u.IsActive
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<UserListItemResult>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Users
            .AsNoTracking()
            .Select(u => new UserListItemResult
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role,
                IsActive = u.IsActive
            })
            .ToListAsync(ct);
    }

    public async Task<CreateUserResult> CreateAsync(
        string username,
        string password,
        string role,
        CancellationToken ct = default)
    {
        var normalizedUsername = username.Trim().ToLower();

        var exists = await _db.Users
            .AnyAsync(u => u.Username.ToLower() == normalizedUsername, ct);

        if (exists)
        {
            return new CreateUserResult
            {
                Status = CreateUserStatus.UsernameAlreadyExists
            };
        }

        var user = new UserEntity
        {
            Username = username.Trim(),
            PasswordHash = _passwordHasher.Hash(password),
            Role = role,
            IsActive = true
        };

        try
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);

            return new CreateUserResult
            {
                Status = CreateUserStatus.Success,
                Id = user.Id,
                Username = user.Username,
                Role = user.Role,
                IsActive = user.IsActive
            };
        }
        catch (DbUpdateException)
        {
            return new CreateUserResult
            {
                Status = CreateUserStatus.Error
            };
        }
    }

    public async Task<UpdateUserRoleStatus> UpdateRoleAsync(
        Guid id,
        string role,
        CancellationToken ct = default)
    {
        var user = await _db.Users.FindAsync([id], ct);
        if (user == null)
            return UpdateUserRoleStatus.NotFound;

        user.Role = role;
        await _db.SaveChangesAsync(ct);

        return UpdateUserRoleStatus.Success;
    }

    public async Task<DeleteUserStatus> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _db.Users.FindAsync([id], ct);
        if (user == null)
            return DeleteUserStatus.NotFound;

        user.IsActive = false;
        await _db.SaveChangesAsync(ct);

        return DeleteUserStatus.Success;
    }
}