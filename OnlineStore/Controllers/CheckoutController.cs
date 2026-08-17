using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Filters;
using OnlineStore.Models;
using OnlineStore.Models.ViewModels;

namespace OnlineStore.Controllers
{
    [RequireCustomer]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CheckoutController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var user = GetCurrentUser();
            var viewModel = new CheckoutViewModel
            {
                FullName = user?.Name ?? string.Empty,
                Email = user?.Email ?? string.Empty,
                Phone = user?.Phone ?? string.Empty,
                Address = user?.Address ?? string.Empty,
                Cart = BuildCartViewModel()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(CheckoutViewModel viewModel)
        {
            viewModel.Cart = BuildCartViewModel();
            if (!viewModel.Cart.Items.Any())
            {
                ModelState.AddModelError("", "Your cart is empty.");
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = new Orders
            {
                UserId = userId.Value,
                OrderDate = DateTime.Now,
                TotalPrice = viewModel.Cart.Total,
                Status = "Processing"
            };
            _context.Orders.Add(order);
            _context.SaveChanges();

            foreach (var cartItem in viewModel.Cart.Items)
            {
                if (cartItem.Product == null)
                {
                    continue;
                }

                _context.OrderItems.Add(new OrderItems
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Product.Price
                });

                cartItem.Product.Stock = Math.Max(0, cartItem.Product.Stock - cartItem.Quantity);
                _context.CartItems.Remove(cartItem);
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Success), new { id = order.Id });
        }

        public IActionResult Success(int id)
        {
            var order = _context.Orders
                .Include(item => item.User)
                .Include(item => item.OrderItems)
                    .ThenInclude(item => item.Product)
                .FirstOrDefault(item => item.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        private Users? GetCurrentUser()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            return userId.HasValue
                ? _context.Users.FirstOrDefault(user => user.Id == userId.Value)
                : null;
        }

        private CartViewModel BuildCartViewModel()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                throw new InvalidOperationException("A user session is required to checkout.");
            }

            var cart = _context.Carts.FirstOrDefault(item => item.UserId == userId.Value);
            if (cart == null)
            {
                cart = new Cart { UserId = userId.Value, CreatedAt = DateTime.Now };
                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            var items = _context.CartItems
                .Include(item => item.Product)
                .Where(item => item.CartId == cart.Id)
                .ToList();

            return new CartViewModel
            {
                Cart = cart,
                Items = items
            };
        }
    }
}
