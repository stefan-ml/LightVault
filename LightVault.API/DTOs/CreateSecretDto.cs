using System.ComponentModel.DataAnnotations;
namespace LightVault.API.DTOs;

public class CreateSecretRequest
{
    [Required(ErrorMessage = "name is required.")]
    [StringLength(100, MinimumLength = 3, 
        ErrorMessage = "name must be between 3 and 100 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_\-\.]+$", 
        ErrorMessage = "name can contain only letters, numbers, _, -, .")]
    public string Name { get; set; } = default!;

    [Required(ErrorMessage = "value is required.")]
    [StringLength(5000, MinimumLength = 1, ErrorMessage = 
        "value must be between 1 and 5000 characters.")]
    public string Value { get; set; } = default!;
}