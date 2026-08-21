using System.ComponentModel.DataAnnotations;
using OnlineStore.Constants;

namespace OnlineStore.Models.ViewModels;

public class ReviewInputViewModel
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [StringLength(StoreSettings.MaximumReviewLength)]
    public string Comment { get; set; } = string.Empty;
}
