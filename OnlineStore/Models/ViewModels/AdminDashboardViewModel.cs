namespace OnlineStore.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int ProductCount { get; set; }
        public int UserCount { get; set; }
        public int OrderCount { get; set; }
        public int PendingOrderCount { get; set; }
        public int LowStockCount { get; set; }
        public decimal Revenue { get; set; }
        public List<Orders> RecentOrders { get; set; } = new();
        public List<Products> LowStockProducts { get; set; } = new();
    }
}
