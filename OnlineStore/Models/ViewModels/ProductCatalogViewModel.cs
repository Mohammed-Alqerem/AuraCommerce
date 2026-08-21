namespace OnlineStore.Models.ViewModels
{
    public class ProductCatalogViewModel
    {
        public List<Products> Products { get; set; } = new();
        public List<Categories> Categories { get; set; } = new();
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public string Title { get; set; } = "All Products";
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
