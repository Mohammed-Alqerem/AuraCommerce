namespace OnlineStore.Constants;

public static class OrderStatuses
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Shipped = "Shipped";
    public const string Delivered = "Delivered";
    public const string Cancelled = "Cancelled";

    public static readonly IReadOnlyList<string> All =
        [Pending, Processing, Shipped, Delivered, Cancelled];

    public static bool IsValid(string? status) => status is not null && All.Contains(status);
}
