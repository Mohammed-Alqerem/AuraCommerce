using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
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
                .Include(item => item.Images)
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
                ,
                Sku = product.Sku
                ,
                Brand = product.Brand
                ,
                IsFeatured = product.IsFeatured
                ,
                LowStockThreshold = product.LowStockThreshold
                ,
                AdditionalImageUrls = string.Join(Environment.NewLine, product.Images.OrderBy(image => image.SortOrder).Select(image => image.Url))
            };
        }

        await PopulateCategoriesAsync(model.CategoryId, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProductForm(ProductFormViewModel model, CancellationToken cancellationToken)
    {
        model.Sku = model.Sku?.Trim();
        model.Brand = model.Brand?.Trim();
        var category = await _context.Categories.AsNoTracking()
            .Where(item => item.Id == model.CategoryId)
            .Select(item => new { item.IsActive })
            .FirstOrDefaultAsync(cancellationToken);
        if (category is null)
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Select a valid category.");
        }
        else if (model.IsActive && !category.IsActive)
        {
            ModelState.AddModelError(nameof(model.CategoryId), "An active product must use an active category.");
        }
        if (!string.IsNullOrEmpty(model.Sku) && await ProductSkuExistsAsync(model.Sku, model.Id, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.Sku), "This SKU is already assigned to another product.");
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
        product.Sku = model.Sku ?? string.Empty;
        product.Brand = model.Brand ?? string.Empty;
        product.IsFeatured = model.IsFeatured;
        product.LowStockThreshold = model.LowStockThreshold;

        var imageUrls = (model.AdditionalImageUrls ?? string.Empty).Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (imageUrls.Any(url => !Uri.TryCreate(url, UriKind.Absolute, out _)))
        {
            ModelState.AddModelError(nameof(model.AdditionalImageUrls), "Each image URL must be an absolute URL on its own line.");
            await PopulateCategoriesAsync(model.CategoryId, cancellationToken);
            return View(model);
        }
        if (product.Id != 0)
        {
            var existingImages = await _context.ProductImages.Where(image => image.ProductId == product.Id).ToListAsync(cancellationToken);
            _context.ProductImages.RemoveRange(existingImages);
        }
        product.Images = imageUrls.Select((url, index) => new ProductImage
        {
            Url = url,
            AltText = product.Name,
            SortOrder = index
        }).ToList();

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            !string.IsNullOrEmpty(model.Sku) &&
            exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            _logger.LogWarning(exception, "Admin product save conflicted for SKU {Sku}.", model.Sku);
            ModelState.AddModelError(nameof(model.Sku), "This SKU is already assigned to another product.");
            await PopulateCategoriesAsync(model.CategoryId, cancellationToken);
            return View(model);
        }
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

    public async Task<IActionResult> OrderDetails(int id, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.AsNoTracking()
            .Include(item => item.User)
            .Include(item => item.OrderItems)
            .Include(item => item.StatusHistory.OrderBy(history => history.CreatedAt))
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return order is null ? NotFound() : View(order);
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

        if (string.Equals(order.Status, status, StringComparison.Ordinal))
        {
            TempData["AdminMessage"] = $"Order #{order.Id} is already {status.ToLowerInvariant()}.";
            return RedirectToAction(nameof(Orders));
        }

        order.Status = status;
        order.StatusHistory.Add(new OrderStatusHistory { Status = status, Note = "Status updated by store administrator" });
        _context.StoreNotifications.Add(new StoreNotification
        {
            UserId = order.UserId,
            Title = $"Order #{order.Id} updated",
            Message = $"Your order is now {status.ToLowerInvariant()}.",
            Link = $"/Orders/Details/{order.Id}"
        });
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
            .Where(item => item.IsActive || item.Id == selectedCategoryId)
            .OrderBy(item => item.Name)
            .ToListAsync(cancellationToken);
        ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedCategoryId);
    }

    private Task<bool> ProductSkuExistsAsync(string sku, int productId, CancellationToken cancellationToken) =>
        _context.Products.AsNoTracking().AnyAsync(
            product => product.Sku == sku && product.Id != productId,
            cancellationToken);
}
