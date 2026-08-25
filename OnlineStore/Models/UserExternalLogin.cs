using System.ComponentModel.DataAnnotations;

namespace OnlineStore.Models;

public class UserExternalLogin
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required]
    [StringLength(32)]
    public string Provider { get; set; } = string.Empty;

    [Required]
    [StringLength(256)]
    public string ProviderKey { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Users User { get; set; } = null!;
}
