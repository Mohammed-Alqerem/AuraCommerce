using System.ComponentModel.DataAnnotations;

namespace OnlineStore.Models.ViewModels;

public class RegisterViewModel
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Phone]
    [StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(200)]
    public string Address { get; set; } = string.Empty;
}
