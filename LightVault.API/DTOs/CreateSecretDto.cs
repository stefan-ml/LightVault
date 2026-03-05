namespace LightVault.API.DTOs;
public class CreateSecretRequest
{
    public string Name { get; set; } = default!;
    public string Value { get; set; } = default!;
}