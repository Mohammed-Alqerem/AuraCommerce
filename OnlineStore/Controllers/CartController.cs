using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Extensions;
using OnlineStore.Filters;
using OnlineStore.Models;
using OnlineStore.Models.ViewModels;

namespace OnlineStore.Controllers;

[RequireCustomer]
public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CartController> _logger;

    public CartController(ApplicationDbContext context, ILogger<CartController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await BuildCartViewModelAsync(cancellationToken));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1, CancellationToken cancellationToken = default)
    {
        if (quantity < 1)
        {
            return BadRequest();
        }

        var cart = await GetOrCreateCartAsync(cancellationToken);
        var product = await _context.Products
            .FirstOrDefaultAsync(item => item.Id == productId && item.IsActive, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        var item = await _context.CartItems.FirstOrDefaultAsync(cartItem =>
            cartItem.CartId == cart.Id && cartItem.ProductId == productId,
            cancellationToken);
        var currentQuantity = item?.Quantity ?? 0;
        var availableQuantity = product.Stock - currentQuantity;

        if (availableQuantity <= 0)
        {
            TempData["CartMessage"] = $"{product.Name} is already at the available stock limit in your cart.";
            return RedirectToAction(nameof(Index));
        }

        var quantityToAdd = Math.Min(quantity, availableQuantity);
        if (item is null)
        {
            _context.CartItems.Add(new CartItems
            {
                CartId = cart.Id,
                ProductId = productId,
                Quantity = quantityToAdd
            });
        }
        else
        {
            item.Quantity += quantityToAdd;
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            TempData["CartMessage"] = $"{product.Name} was added to your cart.";
        }
        catch (DbUpdateException exception)
        {
            _logger.LogWarning(exception, "Cart add conflicted for cart {CartId} and product {ProductId}.", cart.Id, productId);
            TempData["CartMessage"] = "The cart changed in another request. Please try again.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int itemId, int quantity, CancellationToken cancellationToken)
    {
        var item = await GetCurrentUsersCartItemAsync(itemId, includeProduct: true, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        if (quantity <= 0)
        {
            _context.CartItems.Remove(item);
        }
        else if (item.Product is null || !item.Product.IsActive || item.Product.Stock <= 0)
        {
            _context.CartItems.Remove(item);
            TempData["CartMessage"] = "That product is no longer available and was removed from your cart.";
        }
        else
        {
            item.Quantity = Math.Min(quantity, item.Product.Stock);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int itemId, CancellationToken cancellationToken)
    {
        var item = await GetCurrentUsersCartItemAsync(itemId, cancellationToken: cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    private async Task<CartViewModel> BuildCartViewModelAsync(CancellationToken cancellationToken)
    {
        var cart = await GetOrCreateCartAsync(cancellationToken);
        var items = await _context.CartItems
            .AsNoTracking()
            .Include(item => item.Product)
                .ThenInclude(product => product!.Category)
            .Where(item => item.CartId == cart.Id)
            .OrderBy(item => item.Id)
            .ToListAsync(cancellationToken);
        return new CartViewModel { Cart = cart, Items = items };
    }

    private async Task<Cart> GetOrCreateCartAsync(CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetCurrentUserId()!.Value;
        var cart = await _context.Carts.FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken);
        if (cart is not null)
        {
            return cart;
        }

        cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync(cancellationToken);
        return cart;
    }

    private async Task<CartItems?> GetCurrentUsersCartItemAsync(
        int itemId,
        bool includeProduct = false,
        CancellationToken cancellationToken = default)
    {
        var userId = HttpContext.Session.GetCurrentUserId()!.Value;
        var query = _context.CartItems
            .Where(cartItem => cartItem.Id == itemId && cartItem.Cart!.UserId == userId);
        if (includeProduct)
        {
            query = query.Include(cartItem => cartItem.Product);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}
