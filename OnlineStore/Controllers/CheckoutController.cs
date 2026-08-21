using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Extensions;
using OnlineStore.Filters;
using OnlineStore.Models.ViewModels;
using OnlineStore.Services;

namespace OnlineStore.Controllers;

[RequireCustomer]
public class CheckoutController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICheckoutService _checkoutService;

    public CheckoutController(ApplicationDbContext context, ICheckoutService checkoutService)
    {
        _context = context;
        _checkoutService = checkoutService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetCurrentUserId()!.Value;
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        var viewModel = new CheckoutViewModel
        {
            FullName = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            Cart = await _checkoutService.GetCartAsync(userId, cancellationToken)
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutViewModel viewModel, CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetCurrentUserId()!.Value;
        viewModel.Cart = await _checkoutService.GetCartAsync(userId, cancellationToken);

        if (viewModel.Cart.Items.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Your cart is empty.");
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var result = await _checkoutService.CheckoutAsync(userId, cancellationToken);
        if (!result.Succeeded)
        {
            viewModel.Cart = await _checkoutService.GetCartAsync(userId, cancellationToken);
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Checkout could not be completed.");
            return View(viewModel);
        }

        return RedirectToAction(nameof(Success), new { id = result.OrderId });
    }

    [HttpGet]
    public async Task<IActionResult> Success(int id, CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetCurrentUserId()!.Value;
        var order = await _context.Orders
            .AsNoTracking()
            .Include(item => item.User)
            .Include(item => item.OrderItems)
                .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

        return order is null ? NotFound() : View(order);
    }
}
