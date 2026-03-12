using System.ComponentModel.DataAnnotations;

namespace LightVault.API.DTOs
{
    public class RotateSecretRequest
    {
        [Required(ErrorMessage = "value is required.")]
        [StringLength(5000, MinimumLength = 1, ErrorMessage =
        "value must be between 1 and 5000 characters.")]
        public string NewValue { get; set; } = default!;
    }

}
