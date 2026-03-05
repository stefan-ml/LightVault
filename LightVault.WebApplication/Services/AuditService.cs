using LightVault.WebApplication.Models;

namespace LightVault.WebApplication.Services;

public class AuditService
{
    private readonly ApiClient _api;

    public AuditService(ApiClient api)
    {
        _api = api;
    }

    public Task<List<AuditEntryDto>?> GetAll()
        => _api.GetAsync<List<AuditEntryDto>>("/api/audit");

    public Task<List<AuditEntryDto>?> Filter(string? user, string? action, DateTime? from, DateTime? to)
    {
        string query = $"/api/audit/filter" +
                       $"?user={user}" +
                       $"&action={action}" +
                       $"&from={(from.HasValue ? from.Value.ToString("O") : null)}" +
                       $"&to={(to.HasValue ? to.Value.ToString("O") : null)}";

        return _api.GetAsync<List<AuditEntryDto>>(query);
    }
}
