using System.ComponentModel.DataAnnotations;

namespace LightVault.API.DTOs
{
    public class CreateUserRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression("^[a-zA-Z0-9._-]+$", ErrorMessage = "Username contains invalid characters.")]
        public string Username { get; set; } = default!;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        [Required]
        [RegularExpression("^(Admin|Developer|Auditor)$", ErrorMessage = "Invalid role.")]
        public string Role { get; set; } = "Developer";
    }
}