using System.ComponentModel.DataAnnotations;

namespace OnlineStore.Models;

public class SupportTicket
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public Users? User { get; set; }
    [Required, StringLength(80)] public string Name { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = string.Empty;
    [Required, StringLength(120)] public string Subject { get; set; } = string.Empty;
    [Required, StringLength(2000)] public string Message { get; set; } = string.Empty;
    [Required, StringLength(20)] public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
