using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineStore.Models
{
    public class Products
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 100000)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, 100000)]
        public int Stock { get; set; }

        [Url]
        [StringLength(2048)]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        [StringLength(50)]
        public string Sku { get; set; } = string.Empty;

        [StringLength(80)]
        public string Brand { get; set; } = string.Empty;

        public bool IsFeatured { get; set; }

        public int LowStockThreshold { get; set; } = 10;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Categories? Category { get; set; }

        public ICollection<OrderItems> OrderItems { get; set; } = new List<OrderItems>();
        public ICollection<CartItems> CartItems { get; set; } = new List<CartItems>();
        public ICollection<Reviews> Reviews { get; set; } = new List<Reviews>();
        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<InventoryAdjustment> InventoryAdjustments { get; set; } = new List<InventoryAdjustment>();

    }
}
