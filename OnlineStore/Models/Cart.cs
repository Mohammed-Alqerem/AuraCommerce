using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineStore.Models
{
    public class Cart
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public Users? User { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<CartItems> CartItems { get; set; } = new List<CartItems>();
    }
}
