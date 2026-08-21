using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Constants;
using OnlineStore.Data;
using OnlineStore.Filters;
using OnlineStore.Models;
using OnlineStore.Models.ViewModels;

namespace OnlineStore.Controllers;

[RequireAdmin]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var viewModel = new AdminDashboardViewModel
        {
            ProductCount = await _context.Products.CountAsync(product => product.IsActive, cancellationToken),
            UserCount = await _context.Users.CountAsync(user => user.Role == UserRoles.Customer, cancellationToken),
            OrderCount = await _context.Orders.CountAsync(cancellationToken),
            PendingOrderCount = await _context.Orders.CountAsync(
                order => order.Status == OrderStatuses.Pending || order.Status == OrderStatuses.Processing,
                cancellationToken),
            LowStockCount = await _context.Products.CountAsync(
                product => product.IsActive && product.Stock <= StoreSettings.LowStockThreshold,
                cancellationToken),
            Revenue = await _context.Orders
                .Where(order => order.Status != OrderStatuses.Cancelled)
                .SumAsync(order => (decimal?)order.TotalPrice, cancellationToken) ?? 0m,
            RecentOrders = await _context.Orders
                .AsNoTracking()
                .Include(order => order.User)
                .OrderByDescending(order => order.OrderDate)
                .Take(6)
                .ToListAsync(cancellationToken),
            LowStockProducts = await _context.Products
                .AsNoTracking()
                .Include(product => product.Category)
                .Where(product => product.IsActive && product.Stock <= StoreSettings.LowStockThreshold)
                .OrderBy(product => product.Stock)
                .Take(6)
                .ToListAsync(cancellationToken)
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Products(CancellationToken cancellationToken)
    {
        var products = await _context.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .OrderByDescending(product => product.IsActive)
            .ThenBy(product => product.Name)
            .ToListAsync(cancellationToken);

        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> ProductForm(int? id, CancellationToken cancellationToken)
    {
        var model = new ProductFormViewModel();
        if (id.HasValue)
        {
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id.Value, cancellationToken);
            if (product is null)
            {
                return NotFound();
            }

            model = new ProductFormViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                IsActive = product.IsActive
            };
        }

        await PopulateCategoriesAsync(model.CategoryId, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProductForm(ProductFormViewModel model, CancellationToken cancellationToken)
    {
        if (!await _context.Categories.AsNoTracking().AnyAsync(item => item.Id == model.CategoryId, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Select a valid category.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(model.CategoryId, cancellationToken);
            return View(model);
        }

        Products product;
        if (model.Id == 0)
        {
            product = new Products();
            _context.Products.Add(product);
        }
        else
        {
            var existingProduct = await _context.Products.FirstOrDefaultAsync(item => item.Id == model.Id, cancellationToken);
            if (existingProduct is null)
            {
                return NotFound();
            }

            product = existingProduct;
        }

        product.Name = model.Name.Trim();
        product.Description = model.Description.Trim();
        product.Price = model.Price;
        product.Stock = model.Stock;
        product.ImageUrl = model.ImageUrl?.Trim() ?? string.Empty;
        product.CategoryId = model.CategoryId;
        product.IsActive = model.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Admin saved product {ProductId}.", product.Id);
        TempData["AdminMessage"] = $"{product.Name} was saved.";
        return RedirectToAction(nameof(Products));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        product.IsActive = !product.IsActive;
        if (!product.IsActive)
        {
            var cartItems = await _context.CartItems.Where(item => item.ProductId == id).ToListAsync(cancellationToken);
            _context.CartItems.RemoveRange(cartItems);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Admin changed product {ProductId} active state to {IsActive}.", product.Id, product.IsActive);
        TempData["AdminMessage"] = product.IsActive
            ? $"{product.Name} was restored."
            : $"{product.Name} was archived without deleting order history.";
        return RedirectToAction(nameof(Products));
    }

    public async Task<IActionResult> Orders(CancellationToken cancellationToken)
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Include(order => order.User)
            .Include(order => order.OrderItems)
            .OrderByDescending(order => order.OrderDate)
            .ToListAsync(cancellationToken);
        return View(orders);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrderStatus(int id, string status, CancellationToken cancellationToken)
    {
        if (!OrderStatuses.IsValid(status))
        {
            return BadRequest();
        }

        var order = await _context.Orders.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        order.Status = status;
        await _context.SaveChangesAsync(cancellationToken);
        TempData["AdminMessage"] = $"Order #{order.Id} is now {status.ToLowerInvariant()}.";
        return RedirectToAction(nameof(Orders));
    }

    public async Task<IActionResult> Users(CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .AsNoTracking()
            .Where(user => user.Role == UserRoles.Customer)
            .Include(user => user.Orders)
            .OrderBy(user => user.Name)
            .ToListAsync(cancellationToken);
        return View(users);
    }

    private async Task PopulateCategoriesAsync(int selectedCategoryId, CancellationToken cancellationToken)
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(item => item.Name)
            .ToListAsync(cancellationToken);
        ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedCategoryId);
    }
}
