namespace OnlineStore.Models.ViewModels
{
    public class StoreHomeViewModel
    {
        public List<Products> FeaturedProducts { get; set; } = new();
        public List<Categories> Categories { get; set; } = new();
        public int ProductCount { get; set; }
        public int OrderCount { get; set; }
    }
}
