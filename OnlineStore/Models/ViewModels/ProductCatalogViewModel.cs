namespace OnlineStore.Models.ViewModels
{
    public class ProductCatalogViewModel
    {
        public List<Products> Products { get; set; } = new();
        public List<Categories> Categories { get; set; } = new();
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public string Title { get; set; } = "All Products";
    }
}
