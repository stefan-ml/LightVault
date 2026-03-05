using LightVault.WebApplication.Models;

namespace LightVault.WebApplication.Services;

public class SecretService
{
    private readonly ApiClient _api;

    public SecretService(ApiClient api)
    {
        _api = api;
    }

    public Task<List<SecretDto>?> GetAll()
        => _api.GetAsync<List<SecretDto>>("/api/secrets");

    public Task<SecretDetailsDto?> GetById(Guid id)
        => _api.GetAsync<SecretDetailsDto>($"/api/secrets/{id}");

    public async Task<Guid?> Create(CreateSecretRequest req)
    {
        var id = await _api.PostAsync<Guid>("/api/secrets", req);
        return id;
    }

    public Task<SecretDetailsDto?> AddVersion(Guid id, AddSecretVersionRequest req)
        => _api.PostAsync<SecretDetailsDto>($"/api/secrets/{id}/versions", req);

    public Task Delete(Guid id)
        => _api.DeleteAsync($"/api/secrets/{id}");

    public async Task<SecretDetailsDto?> GetVersion(Guid id, int version)
    {
        return await _api.GetAsync<SecretDetailsDto>($"api/secrets/{id}/versions/{version}");
    }

    public Task Rotate(Guid id, RotateSecretRequest req)
    => _api.PostAsync<object>($"/api/secrets/{id}/rotate", req);

}
