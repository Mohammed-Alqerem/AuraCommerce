using System.ComponentModel.DataAnnotations;

namespace OnlineStore.Models;

public class InventoryAdjustment
{
    public int Id { get; set; }
    [Required] public int ProductId { get; set; }
    public Products? Product { get; set; }
    public int QuantityChange { get; set; }
    public int StockAfter { get; set; }
    [Required, StringLength(160)] public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
