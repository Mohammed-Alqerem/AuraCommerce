using System.ComponentModel.DataAnnotations;

namespace OnlineStore.Models
{
    public class Categories
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        public ICollection<Products> Products { get; set; }
    }
}
