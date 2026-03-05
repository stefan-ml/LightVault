using System.ComponentModel.DataAnnotations;

namespace LightVault.WebApplication.Models
{
    public class RotateSecretRequest
    {
        [Required(ErrorMessage = "Secret value is required.")]
        [MinLength(1, ErrorMessage = "Secret value cannot be empty.")]
        public string NewValue { get; set; } = string.Empty;
    }
}
