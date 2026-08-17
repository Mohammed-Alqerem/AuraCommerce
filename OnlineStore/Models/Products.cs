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
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Categories? Category { get; set; }

        public ICollection<OrderItems> OrderItems { get; set; } = new List<OrderItems>();
        public ICollection<CartItems> CartItems { get; set; } = new List<CartItems>();
        public ICollection<Reviews> Reviews { get; set; } = new List<Reviews>();

    }
}
