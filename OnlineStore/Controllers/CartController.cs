using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Filters;
using OnlineStore.Models;
using OnlineStore.Models.ViewModels;

namespace OnlineStore.Controllers
{
    [RequireCustomer]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(BuildCartViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int productId, int quantity = 1)
        {
            var cart = GetOrCreateCart();
            var product = _context.Products.FirstOrDefault(item => item.Id == productId);
            if (product == null)
            {
                return NotFound();
            }

            var item = _context.CartItems.FirstOrDefault(cartItem =>
                cartItem.CartId == cart.Id && cartItem.ProductId == productId);

            if (item == null)
            {
                _context.CartItems.Add(new CartItems
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = Math.Max(1, quantity)
                });
            }
            else
            {
                item.Quantity += Math.Max(1, quantity);
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int itemId, int quantity)
        {
            var item = _context.CartItems.FirstOrDefault(cartItem => cartItem.Id == itemId);
            if (item == null)
            {
                return NotFound();
            }

            if (quantity <= 0)
            {
                _context.CartItems.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int itemId)
        {
            var item = _context.CartItems.FirstOrDefault(cartItem => cartItem.Id == itemId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        private CartViewModel BuildCartViewModel()
        {
            var cart = GetOrCreateCart();
            var items = _context.CartItems
                .Include(item => item.Product)
                    .ThenInclude(product => product!.Category)
                .Where(item => item.CartId == cart.Id)
                .OrderBy(item => item.Id)
                .ToList();

            return new CartViewModel
            {
                Cart = cart,
                Items = items
            };
        }

        private Cart GetOrCreateCart()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                throw new InvalidOperationException("A user session is required to use the cart.");
            }

            var cart = _context.Carts.FirstOrDefault(item => item.UserId == userId.Value);
            if (cart != null)
            {
                return cart;
            }

            cart = new Cart
            {
                UserId = userId.Value,
                CreatedAt = DateTime.Now
            };
            _context.Carts.Add(cart);
            _context.SaveChanges();

            return cart;
        }
    }
}
