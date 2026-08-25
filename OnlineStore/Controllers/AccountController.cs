using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Constants;
using OnlineStore.Data;
using OnlineStore.Extensions;
using OnlineStore.Filters;
using OnlineStore.Models;
using OnlineStore.Models.ViewModels;
using OnlineStore.Services;

namespace OnlineStore.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<Users> _passwordHasher;
    private readonly ILogger<AccountController> _logger;
    private readonly IAccountTokenService _accountTokens;
    private readonly IStoreEmailSender _emailSender;
    private readonly IWebHostEnvironment _environment;
    private readonly IExternalAccountService _externalAccounts;
    private readonly ExternalProviderAvailability _externalProviders;

    public AccountController(
        ApplicationDbContext context,
        IPasswordHasher<Users> passwordHasher,
        ILogger<AccountController> logger,
        IAccountTokenService accountTokens,
        IStoreEmailSender emailSender,
        IWebHostEnvironment environment,
        IExternalAccountService externalAccounts,
        ExternalProviderAvailability externalProviders)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _accountTokens = accountTokens;
        _emailSender = emailSender;
        _environment = environment;
        _externalAccounts = externalAccounts;
        _externalProviders = externalProviders;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null, string? externalError = null)
    {
        if (HttpContext.Session.GetCurrentUserId().HasValue)
        {
            return HttpContext.Session.IsInRole(UserRoles.Admin)
                ? RedirectToAction("Index", "Admin")
                : RedirectToAction("Index", "Products");
        }

        SetReturnUrl(returnUrl);
        if (!string.IsNullOrWhiteSpace(externalError))
        {
            TempData["ExternalLoginMessage"] = "External sign-in was cancelled or could not be completed. You can still sign in with email.";
        }

        var pendingEmail = HttpContext.Session.GetString(SessionKeys.PendingExternalEmail);
        return View(new LoginViewModel { Email = pendingEmail ?? string.Empty });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("authentication")]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            SetReturnUrl(returnUrl);
            return View(model);
        }

        var normalizedEmail = NormalizeEmail(model.Email);
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail);

        if (existingUser is null || !VerifyPassword(existingUser, model.Password, out var verification))
        {
            _logger.LogWarning("Failed login attempt.");
            ModelState.AddModelError(string.Empty, "Email or password is incorrect.");
            SetReturnUrl(returnUrl);
            return View(model);
        }

        if (!UserRoles.IsValid(existingUser.Role))
        {
            _logger.LogWarning("Login blocked for user {UserId} because the stored role is invalid.", existingUser.Id);
            ModelState.AddModelError(string.Empty, "This account is not configured correctly. Please contact support.");
            SetReturnUrl(returnUrl);
            return View(model);
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            existingUser.Password = _passwordHasher.HashPassword(existingUser, model.Password);
            await _context.SaveChangesAsync();
        }

        await LinkPendingExternalLoginAsync(existingUser);

        HttpContext.Session.SignIn(existingUser);
        _logger.LogInformation("User {UserId} signed in with role {Role}.", existingUser.Id, existingUser.Role);

        return RedirectAfterSignIn(existingUser, returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("authentication")]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        if (!ExternalAuthenticationSchemes.IsSupported(provider) || !_externalProviders.IsEnabled(provider))
        {
            TempData["ExternalLoginMessage"] = "That external sign-in option is not available. You can still sign in with email.";
            return RedirectToAction(nameof(Login), new { returnUrl = LocalReturnUrl(returnUrl) });
        }

        ClearPendingExternalLogin();
        var callbackUrl = Url.Action(nameof(ExternalLoginCallback), new
        {
            provider,
            returnUrl = LocalReturnUrl(returnUrl)
        })!;
        return Challenge(new AuthenticationProperties { RedirectUri = callbackUrl }, provider);
    }

    [HttpGet]
    [EnableRateLimiting("authentication")]
    public async Task<IActionResult> ExternalLoginCallback(
        string? provider = null,
        string? returnUrl = null,
        string? remoteError = null,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(remoteError) ||
            !ExternalAuthenticationSchemes.IsSupported(provider) ||
            !_externalProviders.IsEnabled(provider!))
        {
            TempData["ExternalLoginMessage"] = "External sign-in was cancelled or could not be completed. You can still sign in with email.";
            return RedirectToAction(nameof(Login), new { returnUrl = LocalReturnUrl(returnUrl) });
        }

        var authentication = await HttpContext.AuthenticateAsync(ExternalAuthenticationSchemes.ExternalCookie);
        try
        {
            if (!authentication.Succeeded ||
                authentication.Principal is null ||
                !string.Equals(
                    authentication.Principal.Identity?.AuthenticationType,
                    provider,
                    StringComparison.Ordinal))
            {
                TempData["ExternalLoginMessage"] = "External sign-in could not be verified. Please try again or use email.";
                return RedirectToAction(nameof(Login), new { returnUrl = LocalReturnUrl(returnUrl) });
            }

            var providerKey = authentication.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(providerKey))
            {
                TempData["ExternalLoginMessage"] = "External sign-in could not be verified. Please try again or use email.";
                return RedirectToAction(nameof(Login), new { returnUrl = LocalReturnUrl(returnUrl) });
            }

            var identity = new ExternalIdentity(
                provider!,
                providerKey,
                authentication.Principal.FindFirstValue(ClaimTypes.Email),
                HasVerifiedEmail(authentication.Principal),
                authentication.Principal.FindFirstValue(ClaimTypes.Name));
            var resolution = await _externalAccounts.ResolveAsync(identity, cancellationToken);

            if (resolution.Kind == ExternalAccountResolutionKind.SignedIn && resolution.User is not null)
            {
                HttpContext.Session.SignIn(resolution.User);
                _logger.LogInformation("User {UserId} signed in with external provider {Provider}.", resolution.User.Id, provider);
                return RedirectAfterSignIn(resolution.User, returnUrl);
            }

            if (resolution.Kind == ExternalAccountResolutionKind.LinkRequired &&
                !string.IsNullOrWhiteSpace(resolution.NormalizedEmail))
            {
                StorePendingExternalLogin(identity, resolution.NormalizedEmail, returnUrl);
                TempData["ExternalLoginMessage"] = "This email already has an Aura Commerce account. Enter its password once to securely link the accounts.";
                return RedirectToAction(nameof(Login), new { returnUrl = LocalReturnUrl(returnUrl) });
            }

            TempData["ExternalLoginMessage"] = "This external account cannot be used to sign in. Try another method or contact support.";
            return RedirectToAction(nameof(Login), new { returnUrl = LocalReturnUrl(returnUrl) });
        }
        finally
        {
            await HttpContext.SignOutAsync(ExternalAuthenticationSchemes.ExternalCookie);
        }
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost]
    [EnableRateLimiting("authentication")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        var normalizedEmail = NormalizeEmail(model.Email);
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(item => item.NormalizedEmail == normalizedEmail, cancellationToken);
        if (user is not null)
        {
            var token = _accountTokens.Create(user.Id, user.NormalizedEmail, user.SecurityVersion, "password-reset", TimeSpan.FromHours(1));
            var link = Url.Action(nameof(ResetPassword), "Account", new { token }, Request.Scheme)!;
            var sent = await _emailSender.SendAsync(user.Email, "Reset your Aura Commerce password", $"<p><a href=\"{link}\">Reset password</a></p>", cancellationToken);
            if (!sent && _environment.IsDevelopment()) TempData["DevelopmentResetLink"] = link;
        }
        return View("ForgotPasswordSent");
    }

    [HttpGet]
    public IActionResult ResetPassword(string token) => View(new ResetPasswordViewModel { Token = token });

    [HttpPost]
    [EnableRateLimiting("authentication")]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        if (!_accountTokens.TryRead(model.Token, "password-reset", out var payload) || payload is null)
        {
            ModelState.AddModelError(string.Empty, "This password reset link is invalid or expired.");
            return View(model);
        }
        var user = await _context.Users.FirstOrDefaultAsync(item => item.Id == payload.UserId, cancellationToken);
        if (user is null || user.NormalizedEmail != payload.Email || user.SecurityVersion != payload.SecurityVersion)
        {
            ModelState.AddModelError(string.Empty, "This password reset link is invalid or expired.");
            return View(model);
        }
        user.Password = _passwordHasher.HashPassword(user, model.Password);
        user.SecurityVersion++;
        await _context.SaveChangesAsync(cancellationToken);
        TempData["StoreMessage"] = "Your password was reset. You can sign in now.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("authentication")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        model.Name = model.Name?.Trim() ?? string.Empty;
        model.Email = model.Email?.Trim() ?? string.Empty;
        model.Phone = model.Phone?.Trim() ?? string.Empty;
        model.Address = model.Address?.Trim() ?? string.Empty;
        var normalizedEmail = NormalizeEmail(model.Email);

        if (await _context.Users.AnyAsync(user => user.NormalizedEmail == normalizedEmail))
        {
            ModelState.AddModelError(nameof(model.Email), "This email is already registered.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new Users
        {
            Name = model.Name,
            Email = model.Email,
            NormalizedEmail = normalizedEmail,
            Phone = model.Phone,
            Address = model.Address,
            Role = UserRoles.Customer,
            CreatedAt = DateTime.UtcNow,
            Cart = new Cart { CreatedAt = DateTime.UtcNow }
        };
        user.Password = _passwordHasher.HashPassword(user, model.Password);

        _context.Users.Add(user);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
        {
            _logger.LogWarning(exception, "Registration failed because the email could not be stored uniquely.");
            ModelState.AddModelError(nameof(model.Email), "This email is already registered.");
            return View(model);
        }

        HttpContext.Session.SignIn(user);
        _logger.LogInformation("Customer account {UserId} registered.", user.Id);
        return RedirectToAction("Index", "Products");
    }

    [HttpGet]
    [RequireCustomer]
    public async Task<IActionResult> Profile()
    {
        var userId = HttpContext.Session.GetCurrentUserId()!.Value;
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == userId);

        return user is null
            ? RedirectToAction(nameof(Login))
            : View(await CreateProfileViewModelAsync(user));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequireCustomer]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        var userId = HttpContext.Session.GetCurrentUserId()!.Value;
        var user = await _context.Users.FirstOrDefaultAsync(item => item.Id == userId);
        if (user is null)
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        model.Name = model.Name?.Trim() ?? string.Empty;
        model.Email = model.Email?.Trim() ?? string.Empty;
        model.Phone = model.Phone?.Trim() ?? string.Empty;
        model.Address = model.Address?.Trim() ?? string.Empty;
        var normalizedEmail = NormalizeEmail(model.Email);

        if (await _context.Users.AnyAsync(item =>
                item.NormalizedEmail == normalizedEmail && item.Id != userId))
        {
            ModelState.AddModelError(nameof(model.Email), "This email is already registered.");
        }

        if (!string.IsNullOrWhiteSpace(model.NewPassword) &&
            (string.IsNullOrWhiteSpace(model.CurrentPassword) ||
             !VerifyPassword(user, model.CurrentPassword, out _)))
        {
            ModelState.AddModelError(nameof(model.CurrentPassword), "Enter your current password to set a new password.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateProfileDetailsAsync(model, user.Id, user.CreatedAt);
            return View(model);
        }

        var invalidateSecurityTokens = false;
        if (!string.Equals(user.NormalizedEmail, normalizedEmail, StringComparison.Ordinal))
        {
            user.EmailConfirmed = false;
            invalidateSecurityTokens = true;
        }
        user.Name = model.Name;
        user.Email = model.Email;
        user.NormalizedEmail = normalizedEmail;
        user.Phone = model.Phone;
        user.Address = model.Address;

        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            user.Password = _passwordHasher.HashPassword(user, model.NewPassword);
            invalidateSecurityTokens = true;
        }
        if (invalidateSecurityTokens) user.SecurityVersion++;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
        {
            _logger.LogWarning(exception, "Profile update for user {UserId} violated a database constraint.", user.Id);
            ModelState.AddModelError(nameof(model.Email), "This email is already registered.");
            await PopulateProfileDetailsAsync(model, user.Id, user.CreatedAt);
            return View(model);
        }

        HttpContext.Session.SignIn(user);
        TempData["ProfileSaved"] = "Your profile has been updated.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [RequireCustomer]
    [EnableRateLimiting("authentication")]
    public async Task<IActionResult> SendEmailVerification(CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetCurrentUserId()!.Value;
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null) return RedirectToAction(nameof(Login));
        if (user.EmailConfirmed) return RedirectToAction(nameof(Profile));
        var token = _accountTokens.Create(user.Id, user.NormalizedEmail, user.SecurityVersion, "email-confirmation", TimeSpan.FromHours(24));
        var link = Url.Action(nameof(ConfirmEmail), "Account", new { token }, Request.Scheme)!;
        var sent = await _emailSender.SendAsync(user.Email, "Confirm your Aura Commerce email", $"<p><a href=\"{link}\">Confirm email</a></p>", cancellationToken);
        TempData["StoreMessage"] = sent ? "Verification email sent." : "Email delivery is not configured.";
        if (!sent && _environment.IsDevelopment()) TempData["DevelopmentVerificationLink"] = link;
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string token, CancellationToken cancellationToken)
    {
        if (!_accountTokens.TryRead(token, "email-confirmation", out var payload) || payload is null) return BadRequest("Invalid or expired verification link.");
        var user = await _context.Users.FirstOrDefaultAsync(item => item.Id == payload.UserId, cancellationToken);
        if (user is null || user.NormalizedEmail != payload.Email || user.SecurityVersion != payload.SecurityVersion) return BadRequest("Invalid or expired verification link.");
        user.EmailConfirmed = true;
        await _context.SaveChangesAsync(cancellationToken);
        TempData["StoreMessage"] = "Your email address is confirmed.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequireLogin]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    private bool VerifyPassword(Users user, string password, out PasswordVerificationResult result)
    {
        try
        {
            result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
            return result != PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            result = PasswordVerificationResult.Failed;
            return false;
        }
    }

    private async Task LinkPendingExternalLoginAsync(Users user)
    {
        var provider = HttpContext.Session.GetString(SessionKeys.PendingExternalProvider);
        var providerKey = HttpContext.Session.GetString(SessionKeys.PendingExternalProviderKey);
        var email = HttpContext.Session.GetString(SessionKeys.PendingExternalEmail);
        if (!ExternalAuthenticationSchemes.IsSupported(provider) ||
            string.IsNullOrWhiteSpace(providerKey) ||
            string.IsNullOrWhiteSpace(email))
        {
            ClearPendingExternalLogin();
            return;
        }

        if (!string.Equals(user.NormalizedEmail, email, StringComparison.Ordinal))
        {
            ClearPendingExternalLogin();
            return;
        }

        var linked = await _externalAccounts.LinkAsync(
            user,
            new ExternalIdentity(provider!, providerKey, email, true, null));
        ClearPendingExternalIdentity();
        if (linked)
        {
            TempData["StoreMessage"] = $"{provider} sign-in is now linked to your account.";
        }
        else
        {
            _logger.LogWarning("External account linking was not completed for user {UserId}.", user.Id);
        }
    }

    private void StorePendingExternalLogin(ExternalIdentity identity, string normalizedEmail, string? returnUrl)
    {
        HttpContext.Session.SetString(SessionKeys.PendingExternalProvider, identity.Provider);
        HttpContext.Session.SetString(SessionKeys.PendingExternalProviderKey, identity.ProviderKey);
        HttpContext.Session.SetString(SessionKeys.PendingExternalEmail, normalizedEmail);
        var localReturnUrl = LocalReturnUrl(returnUrl);
        if (localReturnUrl is not null)
        {
            HttpContext.Session.SetString(SessionKeys.PendingExternalReturnUrl, localReturnUrl);
        }
    }

    private void ClearPendingExternalLogin()
    {
        ClearPendingExternalIdentity();
        HttpContext.Session.Remove(SessionKeys.PendingExternalReturnUrl);
    }

    private void ClearPendingExternalIdentity()
    {
        HttpContext.Session.Remove(SessionKeys.PendingExternalProvider);
        HttpContext.Session.Remove(SessionKeys.PendingExternalProviderKey);
        HttpContext.Session.Remove(SessionKeys.PendingExternalEmail);
    }

    private IActionResult RedirectAfterSignIn(Users user, string? returnUrl)
    {
        var pendingReturnUrl = HttpContext.Session.GetString(SessionKeys.PendingExternalReturnUrl);
        var localReturnUrl = LocalReturnUrl(returnUrl) ?? LocalReturnUrl(pendingReturnUrl);
        HttpContext.Session.Remove(SessionKeys.PendingExternalReturnUrl);
        if (localReturnUrl is not null)
        {
            return LocalRedirect(localReturnUrl);
        }

        return user.Role == UserRoles.Admin
            ? RedirectToAction("Index", "Admin")
            : RedirectToAction("Index", "Products");
    }

    private void SetReturnUrl(string? returnUrl) => ViewData["ReturnUrl"] = LocalReturnUrl(returnUrl);

    private string? LocalReturnUrl(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : null;

    private static bool HasVerifiedEmail(ClaimsPrincipal principal) =>
        principal.Claims
            .Where(claim => string.Equals(claim.Type, "email_verified", StringComparison.OrdinalIgnoreCase))
            .Any(claim => string.Equals(claim.Value, "true", StringComparison.OrdinalIgnoreCase) || claim.Value == "1");

    private async Task<ProfileViewModel> CreateProfileViewModelAsync(Users user)
    {
        var model = new ProfileViewModel
        {
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address
            ,
            EmailConfirmed = user.EmailConfirmed
        };
        await PopulateProfileDetailsAsync(model, user.Id, user.CreatedAt);
        return model;
    }

    private async Task PopulateProfileDetailsAsync(ProfileViewModel model, int userId, DateTime createdAt)
    {
        model.MemberSince = createdAt;
        model.EmailConfirmed = await _context.Users.Where(user => user.Id == userId)
            .Select(user => user.EmailConfirmed).SingleAsync();
        model.OrderCount = await _context.Orders.CountAsync(order => order.UserId == userId);
        model.ReviewCount = await _context.Reviews.CountAsync(review => review.UserId == userId);
        model.RecentOrders = await _context.Orders
            .AsNoTracking()
            .Where(order => order.UserId == userId)
            .Include(order => order.OrderItems)
            .OrderByDescending(order => order.OrderDate)
            .Take(3)
            .ToListAsync();
    }

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
}
