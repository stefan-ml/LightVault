using System.ComponentModel.DataAnnotations;

public class CreateSecretRequest
{
    [Required]
    [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Name can contain only letters, numbers, _, -, .")]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Value { get; set; } = string.Empty;
}