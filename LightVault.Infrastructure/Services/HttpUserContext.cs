using LightVault.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace LightVault.Infrastructure.Services;

public sealed class HttpUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal User =>
        _httpContextAccessor.HttpContext?.User
        ?? throw new InvalidOperationException("No active HTTP context.");

    public string ActorName =>
        User.FindFirst("username")?.Value
        ?? User.FindFirst(ClaimTypes.Name)?.Value
        ?? "Unknown";

    public Guid UserId
    {
        get
        {
            var id =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            return Guid.TryParse(id, out var guid)
                ? guid
                : throw new InvalidOperationException("UserId claim missing or invalid.");
        }
    }

    public bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) => User.IsInRole(role);
}