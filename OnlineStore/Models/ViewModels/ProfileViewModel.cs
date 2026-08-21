using System.ComponentModel.DataAnnotations;

namespace OnlineStore.Models.ViewModels
{
    public class ProfileViewModel
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(30)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        public string? ConfirmNewPassword { get; set; }

        public DateTime MemberSince { get; set; }
        public int OrderCount { get; set; }
        public int ReviewCount { get; set; }
        public List<Orders> RecentOrders { get; set; } = [];
    }
}
