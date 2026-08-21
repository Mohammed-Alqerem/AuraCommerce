using System.ComponentModel.DataAnnotations;

namespace OnlineStore.Models.ViewModels;

public class ProductFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "100000")]
    public decimal Price { get; set; }

    [Range(0, 100000)]
    public int Stock { get; set; }

    [Url]
    [StringLength(2048)]
    public string ImageUrl { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Select a valid category.")]
    public int CategoryId { get; set; }

    public bool IsActive { get; set; } = true;
}
