using System.ComponentModel.DataAnnotations;

namespace OnlineStore.Models.ViewModels;

public sealed class CategoryFormViewModel
{
    public int Id { get; set; }
    [Required, StringLength(50, MinimumLength = 2)] public string Name { get; set; } = string.Empty;
    [StringLength(200)] public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public sealed class InventoryViewModel
{
    public List<Products> Products { get; set; } = [];
    public string? Search { get; set; }
    public string StockState { get; set; } = "all";
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
}

public sealed class InventoryAdjustmentViewModel
{
    [Range(1, int.MaxValue)] public int ProductId { get; set; }
    [Range(-100000, 100000)] public int QuantityChange { get; set; }
    [Required, StringLength(160)] public string Reason { get; set; } = string.Empty;
}

public sealed class ReportsViewModel
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<ProductSalesRow> BestSellers { get; set; } = [];
    public List<CategorySalesRow> CategorySales { get; set; } = [];
}

public sealed record ProductSalesRow(int ProductId, string Name, int Units, decimal Revenue);
public sealed record CategorySalesRow(string Name, int Units, decimal Revenue);

public sealed class SupportTicketViewModel
{
    [Required, StringLength(80)] public string Name { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = string.Empty;
    [Required, StringLength(120)] public string Subject { get; set; } = string.Empty;
    [Required, StringLength(2000)] public string Message { get; set; } = string.Empty;
}
