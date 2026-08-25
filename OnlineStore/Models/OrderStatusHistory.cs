using System.ComponentModel.DataAnnotations;

namespace OnlineStore.Models;

public class OrderStatusHistory
{
    public int Id { get; set; }
    [Required] public int OrderId { get; set; }
    public Orders? Order { get; set; }
    [Required, StringLength(20)] public string Status { get; set; } = string.Empty;
    [StringLength(300)] public string Note { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
