using System.ComponentModel.DataAnnotations;

namespace OnlineStore.Models;

public class StoreNotification
{
    public int Id { get; set; }
    [Required] public int UserId { get; set; }
    public Users? User { get; set; }
    [Required, StringLength(100)] public string Title { get; set; } = string.Empty;
    [Required, StringLength(500)] public string Message { get; set; } = string.Empty;
    [StringLength(500)] public string Link { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
