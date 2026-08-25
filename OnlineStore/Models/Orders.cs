using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using OnlineStore.Constants;

namespace OnlineStore.Models
{
    public class Orders
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public Users? User { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Range(0.01, 1000000)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = OrderStatuses.Pending;

        [Required, StringLength(80)]
        public string ShippingName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(256)]
        public string ShippingEmail { get; set; } = string.Empty;

        [StringLength(30)]
        public string ShippingPhone { get; set; } = string.Empty;

        [Required, StringLength(300)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required, StringLength(40)]
        public string DeliveryMethod { get; set; } = "Standard";

        public DateTime? EstimatedDeliveryDate { get; set; }

        public ICollection<OrderItems> OrderItems { get; set; } = new List<OrderItems>();
        public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    }
}
