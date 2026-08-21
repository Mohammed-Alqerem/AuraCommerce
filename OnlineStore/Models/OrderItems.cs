using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineStore.Models
{
    public class OrderItems
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Orders? Order { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Products? Product { get; set; }

        [Required]
        [StringLength(100)]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, 100000)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
    }
}
