namespace LightVault.WebApplication.Models;

public class CreateSecretRequest
{
    public string Name { get; set; } = default!;
    public string Value { get; set; } = default!;
}
