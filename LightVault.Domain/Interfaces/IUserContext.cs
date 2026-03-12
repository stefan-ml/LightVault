namespace LightVault.Domain.Interfaces;

public interface IUserContext
{
    string ActorName { get; }
    Guid UserId { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}