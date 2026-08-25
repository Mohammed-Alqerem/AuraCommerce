using System.ComponentModel.DataAnnotations;

namespace OnlineStore.Models;

public class ProductImage
{
    public int Id { get; set; }
    [Required] public int ProductId { get; set; }
    public Products? Product { get; set; }
    [Required, Url, StringLength(2048)] public string Url { get; set; } = string.Empty;
    [StringLength(160)] public string AltText { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
