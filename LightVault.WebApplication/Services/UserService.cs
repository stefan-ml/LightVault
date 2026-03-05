using LightVault.WebApplication.Models;

namespace LightVault.WebApplication.Services;

public class UserService
{
    private readonly ApiClient _api;

    public UserService(ApiClient api)
    {
        _api = api;
    }

    public Task<List<UserDto>?> GetAll()
        => _api.GetAsync<List<UserDto>>("/api/users");
    public Task<UserDto?> GetById(Guid id)
    => _api.GetAsync<UserDto>($"/api/users/{id}");

    public Task<UserDto?> Create(CreateUserRequest req)
        => _api.PostAsync<UserDto>("/api/users", req);

    public Task Update(Guid id, UpdateUserRequest req)
    {
        var request = new
        {
            Role = req.Role
        };
        return _api.PutAsync($"/api/users/{id}/role", request);
    }

    public Task Delete(Guid id)
        => _api.DeleteAsync($"/api/users/{id}");
}
