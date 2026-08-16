using System.ComponentModel.DataAnnotations;

namespace OnlineStore.Models
{
    public class Users
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Phone]
        public string Phone { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Orders> Orders { get; set; }
        public Cart Cart { get; set; }
        public ICollection<Reviews> Reviews { get; set; }
    }

}
