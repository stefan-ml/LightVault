using LightVault.Infrastructure.Consts;

namespace LightVault.API.DTOs
{
    public class CreateServiceAccountRequest
    {
        public string AppName { get; set; } = default!;
        public string Role { get; set; } = Roles.ServiceClient;
    }
}