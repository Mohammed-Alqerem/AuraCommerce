using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Filters;
using OnlineStore.Models;
using OnlineStore.Models.ViewModels;

namespace OnlineStore.Controllers
{
    [RequireAdmin]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var viewModel = new AdminDashboardViewModel
            {
                ProductCount = _context.Products.Count(),
                UserCount = _context.Users.Count(),
                OrderCount = _context.Orders.Count(),
                PendingOrderCount = _context.Orders.Count(order => order.Status == "Pending" || order.Status == "Processing"),
                LowStockCount = _context.Products.Count(product => product.Stock <= 10),
                Revenue = _context.Orders.Sum(order => order.TotalPrice),
                RecentOrders = _context.Orders
                    .Include(order => order.User)
                    .OrderByDescending(order => order.OrderDate)
                    .Take(6)
                    .ToList(),
                LowStockProducts = _context.Products
                    .Include(product => product.Category)
                    .Where(product => product.Stock <= 10)
                    .OrderBy(product => product.Stock)
                    .Take(6)
                    .ToList()
            };

            return View(viewModel);
        }

        public IActionResult Products()
        {
            var products = _context.Products
                .Include(product => product.Category)
                .OrderBy(product => product.Name)
                .ToList();

            return View(products);
        }

        [HttpGet]
        public IActionResult ProductForm(int? id)
        {
            Products product = id.HasValue
                ? _context.Products.FirstOrDefault(item => item.Id == id.Value) ?? new Products()
                : new Products();

            ViewBag.Categories = new SelectList(_context.Categories.OrderBy(item => item.Name), "Id", "Name");
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProductForm(Products product)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_context.Categories.OrderBy(item => item.Name), "Id", "Name");
                return View(product);
            }

            if (product.Id == 0)
            {
                _context.Products.Add(product);
            }
            else
            {
                _context.Products.Update(product);
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Products));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(item => item.Id == id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Products));
        }

        public IActionResult Orders()
        {
            var orders = _context.Orders
                .Include(order => order.User)
                .Include(order => order.OrderItems)
                .OrderByDescending(order => order.OrderDate)
                .ToList();

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderStatus(int id, string status)
        {
            var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
            if (!validStatuses.Contains(status))
            {
                return BadRequest();
            }

            var order = _context.Orders.FirstOrDefault(item => item.Id == id);
            if (order != null)
            {
                order.Status = status;
                _context.SaveChanges();
                TempData["AdminMessage"] = $"Order #{order.Id} is now {status.ToLowerInvariant()}.";
            }

            return RedirectToAction(nameof(Orders));
        }

        public IActionResult Users()
        {
            var users = _context.Users
                .Include(user => user.Orders)
                .OrderBy(user => user.Name)
                .ToList();

            return View(users);
        }
    }
}
