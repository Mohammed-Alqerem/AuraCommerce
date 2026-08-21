using System.ComponentModel.DataAnnotations;

using OnlineStore.Constants;

namespace OnlineStore.Models
{
    public class Users
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string NormalizedEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = UserRoles.Customer;

        [Phone]
        [StringLength(30)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Orders> Orders { get; set; } = new List<Orders>();
        public Cart? Cart { get; set; }
        public ICollection<Reviews> Reviews { get; set; } = new List<Reviews>();
    }

}
