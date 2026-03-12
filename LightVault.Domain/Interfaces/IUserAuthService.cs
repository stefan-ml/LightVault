using LightVault.Domain.Models;

namespace LightVault.Domain.Interfaces;

public interface IUserAuthService
{
    Task<UserLoginResult?> LoginAsync(string username, string password, CancellationToken ct = default);
}