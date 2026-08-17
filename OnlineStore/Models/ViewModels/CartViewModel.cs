namespace OnlineStore.Models.ViewModels
{
    public class CartViewModel
    {
        public Cart? Cart { get; set; }
        public List<CartItems> Items { get; set; } = new();
        public decimal Subtotal => Items.Sum(item => (item.Product?.Price ?? 0) * item.Quantity);
        public decimal Shipping => Subtotal > 0 && Subtotal < 75 ? 8.00m : 0m;
        public decimal Tax => Math.Round(Subtotal * 0.07m, 2);
        public decimal Total => Subtotal + Shipping + Tax;
    }
}
