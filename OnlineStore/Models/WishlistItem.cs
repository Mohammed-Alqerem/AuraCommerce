using System.ComponentModel.DataAnnotations;

namespace OnlineStore.Models;

public class WishlistItem
{
    public int Id { get; set; }
    [Required] public int UserId { get; set; }
    public Users? User { get; set; }
    [Required] public int ProductId { get; set; }
    public Products? Product { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
