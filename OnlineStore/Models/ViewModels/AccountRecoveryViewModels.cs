using System.ComponentModel.DataAnnotations;

namespace OnlineStore.Models.ViewModels;

public sealed class ForgotPasswordViewModel
{
    [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = string.Empty;
}

public sealed class ResetPasswordViewModel
{
    [Required] public string Token { get; set; } = string.Empty;
    [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
