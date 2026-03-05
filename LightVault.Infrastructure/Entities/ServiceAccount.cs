using LightVault.Infrastructure.Consts;
using System.ComponentModel.DataAnnotations;

namespace LightVault.Infrastructure.Entities;

public class ServiceAccount
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string AppName { get; set; } = default!;

    [Required]
    [MaxLength(512)]
    public string ApiKeyHash { get; set; } = default!;

    [Required]
    [MaxLength(256)]
    public string ApiKeySalt { get; set; } = default!;

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = Roles.ServiceClient;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedAtUtc { get; set; }
}