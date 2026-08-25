using System.ComponentModel.DataAnnotations;

namespace OnlineStore.Models.ViewModels
{
    public class CheckoutViewModel
    {
        [Required]
        [StringLength(80)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(30)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        public CartViewModel Cart { get; set; } = new();
    }
}
