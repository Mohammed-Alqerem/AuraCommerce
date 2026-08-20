using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using OnlineStore.Data;
using OnlineStore.Filters;
using OnlineStore.Models;
using OnlineStore.Models.ViewModels;

namespace OnlineStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<Users> _passwordHasher;

        public AccountController(ApplicationDbContext context, IPasswordHasher<Users> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (HttpContext.Session.GetInt32("UserId").HasValue)
            {
                return RedirectToAction("Index", "Products");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            var email = model.Email.Trim();
            var existingUser = _context.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());

            if (existingUser == null)
            {
                ModelState.AddModelError("", "Email or Password is incorrect");
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            var legacyPasswordMatches = existingUser.Password == model.Password;
            var verification = PasswordVerificationResult.Failed;

            if (!legacyPasswordMatches)
            {
                try
                {
                    verification = _passwordHasher.VerifyHashedPassword(existingUser, existingUser.Password, model.Password);
                }
                catch (FormatException)
                {
                    // A legacy or malformed persisted value is not an Identity hash.
                    // Treat it as a failed sign-in without exposing an internal error.
                    verification = PasswordVerificationResult.Failed;
                }
            }

            if (verification == PasswordVerificationResult.Failed && !legacyPasswordMatches)
            {
                ModelState.AddModelError("", "Email or Password is incorrect");
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            if (legacyPasswordMatches || verification == PasswordVerificationResult.SuccessRehashNeeded)
            {
                existingUser.Password = _passwordHasher.HashPassword(existingUser, model.Password);
                _context.SaveChanges();
            }

            HttpContext.Session.SetInt32("UserId", existingUser.Id);
            HttpContext.Session.SetString("UserName", existingUser.Name);
            HttpContext.Session.SetString("UserRole", existingUser.Id == 1 ? "Admin" : "Customer");

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            if (existingUser.Id == 1)
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Index", "Products");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new Users());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(Users user)
        {
            user.Email = user.Email.Trim();
            if (_context.Users.Any(existing => existing.Email.ToLower() == user.Email.ToLower()))
            {
                ModelState.AddModelError(nameof(Users.Email), "This email is already registered.");
            }

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            user.CreatedAt = DateTime.Now;
            user.Password = _passwordHasher.HashPassword(user, user.Password);
            _context.Users.Add(user);
            _context.SaveChanges();

            _context.Carts.Add(new Cart
            {
                UserId = user.Id,
                CreatedAt = DateTime.Now
            });
            _context.SaveChanges();

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserRole", "Customer");

            return RedirectToAction("Index", "Products");
        }

        [RequireLogin]
        public IActionResult Profile()
        {
            if (HttpContext.Session.GetInt32("UserId") == 1)
            {
                return RedirectToAction("Index", "Admin");
            }

            var userId = GetCurrentUserId();
            var user = _context.Users
                .Include(u => u.Orders)
                .Include(u => u.Reviews)
                .FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireLogin]
        public IActionResult Profile(Users model)
        {
            if (HttpContext.Session.GetInt32("UserId") == 1)
            {
                return RedirectToAction("Index", "Admin");
            }

            var userId = GetCurrentUserId();
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            ModelState.Remove(nameof(Users.Password));
            model.Email = model.Email.Trim();
            if (_context.Users.Any(existing => existing.Email.ToLower() == model.Email.ToLower() && existing.Id != userId))
            {
                ModelState.AddModelError(nameof(Users.Email), "This email is already registered.");
            }

            if (!ModelState.IsValid)
            {
                model.Orders = user.Orders;
                model.Reviews = user.Reviews;
                model.CreatedAt = user.CreatedAt;
                return View(model);
            }

            user.Name = model.Name.Trim();
            user.Email = model.Email;
            user.Phone = model.Phone;
            user.Address = model.Address;

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                user.Password = _passwordHasher.HashPassword(user, model.Password);
            }

            _context.SaveChanges();
            HttpContext.Session.SetString("UserName", user.Name);
            ViewData["Saved"] = true;

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireLogin]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }
    }
}
