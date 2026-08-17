using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Filters;
using OnlineStore.Models;

namespace OnlineStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
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
        public IActionResult Login(Users user, string? returnUrl = null)
        {
            var existingUser = _context.Users.FirstOrDefault(u =>
                u.Email == user.Email &&
                u.Password == user.Password);

            if (existingUser == null)
            {
                ModelState.AddModelError("", "Email or Password is incorrect");
                return View(user);
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
            if (_context.Users.Any(existing => existing.Email == user.Email))
            {
                ModelState.AddModelError(nameof(Users.Email), "This email is already registered.");
            }

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            user.CreatedAt = DateTime.Now;
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

            user.Name = model.Name;
            user.Email = model.Email;
            user.Phone = model.Phone;
            user.Address = model.Address;

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                user.Password = model.Password;
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
