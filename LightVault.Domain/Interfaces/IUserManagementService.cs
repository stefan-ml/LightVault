using LightVault.Domain.Models;

namespace LightVault.Domain.Interfaces;

public interface IUserManagementService
{
    Task<UserDetailsResult?> GetAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<UserListItemResult>> GetAllAsync(CancellationToken ct = default);

    Task<CreateUserResult> CreateAsync(
        string username,
        string password,
        string role,
        CancellationToken ct = default);

    Task<UpdateUserRoleStatus> UpdateRoleAsync(
        Guid id,
        string role,
        CancellationToken ct = default);

    Task<DeleteUserStatus> DeleteAsync(Guid id, CancellationToken ct = default);
}