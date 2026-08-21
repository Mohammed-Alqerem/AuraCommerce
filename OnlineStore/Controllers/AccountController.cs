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

namespace OnlineStore.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<Users> _passwordHasher;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        ApplicationDbContext context,
        IPasswordHasher<Users> passwordHasher,
        ILogger<AccountController> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (HttpContext.Session.GetCurrentUserId().HasValue)
        {
            return HttpContext.Session.IsInRole(UserRoles.Admin)
                ? RedirectToAction("Index", "Admin")
                : RedirectToAction("Index", "Products");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("authentication")]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        var normalizedEmail = NormalizeEmail(model.Email);
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail);

        if (existingUser is null || !VerifyPassword(existingUser, model.Password, out var verification))
        {
            _logger.LogWarning("Failed login attempt.");
            ModelState.AddModelError(string.Empty, "Email or password is incorrect.");
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        if (!UserRoles.IsValid(existingUser.Role))
        {
            _logger.LogWarning("Login blocked for user {UserId} because the stored role is invalid.", existingUser.Id);
            ModelState.AddModelError(string.Empty, "This account is not configured correctly. Please contact support.");
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            existingUser.Password = _passwordHasher.HashPassword(existingUser, model.Password);
            await _context.SaveChangesAsync();
        }

        HttpContext.Session.SignIn(existingUser);
        _logger.LogInformation("User {UserId} signed in with role {Role}.", existingUser.Id, existingUser.Role);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return existingUser.Role == UserRoles.Admin
            ? RedirectToAction("Index", "Admin")
            : RedirectToAction("Index", "Products");
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

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

        user.Name = model.Name;
        user.Email = model.Email;
        user.NormalizedEmail = normalizedEmail;
        user.Phone = model.Phone;
        user.Address = model.Address;

        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            user.Password = _passwordHasher.HashPassword(user, model.NewPassword);
        }

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

    private async Task<ProfileViewModel> CreateProfileViewModelAsync(Users user)
    {
        var model = new ProfileViewModel
        {
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address
        };
        await PopulateProfileDetailsAsync(model, user.Id, user.CreatedAt);
        return model;
    }

    private async Task PopulateProfileDetailsAsync(ProfileViewModel model, int userId, DateTime createdAt)
    {
        model.MemberSince = createdAt;
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
