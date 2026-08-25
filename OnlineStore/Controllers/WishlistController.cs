using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Extensions;
using OnlineStore.Filters;
using OnlineStore.Models;

namespace OnlineStore.Controllers;

[RequireCustomer]
public class WishlistController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetCurrentUserId()!.Value;
        var items = await context.WishlistItems.AsNoTracking()
            .Where(item => item.UserId == userId)
            .Include(item => item.Product).ThenInclude(product => product!.Category)
            .OrderByDescending(item => item.AddedAt)
            .ToListAsync(cancellationToken);
        return View(items);
    }

    [HttpPost]
    public async Task<IActionResult> Toggle(int productId, string? returnUrl, CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetCurrentUserId()!.Value;
        if (!await context.Products.AnyAsync(product => product.Id == productId && product.IsActive, cancellationToken))
        {
            return NotFound();
        }

        var existing = await context.WishlistItems
            .FirstOrDefaultAsync(item => item.UserId == userId && item.ProductId == productId, cancellationToken);
        if (existing is null)
        {
            context.WishlistItems.Add(new WishlistItem { UserId = userId, ProductId = productId });
            TempData["StoreMessage"] = "Product saved to your wishlist.";
        }
        else
        {
            context.WishlistItems.Remove(existing);
            TempData["StoreMessage"] = "Product removed from your wishlist.";
        }
        await context.SaveChangesAsync(cancellationToken);

        return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction(nameof(Index));
    }
}
