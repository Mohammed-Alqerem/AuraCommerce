using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Constants;
using OnlineStore.Data;
using OnlineStore.Extensions;
using OnlineStore.Filters;
using OnlineStore.Models;
using OnlineStore.Models.ViewModels;

namespace OnlineStore.Controllers;

public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ApplicationDbContext context, ILogger<ProductsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index(
        string? searchTerm,
        int? categoryId,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        searchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();

        var productsQuery = _context.Products
            .AsNoTracking()
            .Where(product => product.IsActive)
            .Include(product => product.Category)
            .Include(product => product.Reviews)
            .AsQueryable();

        if (searchTerm is not null)
        {
            productsQuery = productsQuery.Where(product =>
                product.Name.Contains(searchTerm) ||
                product.Description.Contains(searchTerm) ||
                (product.Category != null && product.Category.Name.Contains(searchTerm)));
        }

        if (categoryId.HasValue)
        {
            productsQuery = productsQuery.Where(product => product.CategoryId == categoryId.Value);
        }

        var totalItems = await productsQuery.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)StoreSettings.ProductsPerPage);
        if (totalPages > 0 && page > totalPages)
        {
            page = totalPages;
        }

        var selectedCategory = categoryId.HasValue
            ? await _context.Categories.AsNoTracking().FirstOrDefaultAsync(category => category.Id == categoryId.Value, cancellationToken)
            : null;

        var viewModel = new ProductCatalogViewModel
        {
            Products = await productsQuery
                .OrderBy(product => product.Name)
                .Skip((page - 1) * StoreSettings.ProductsPerPage)
                .Take(StoreSettings.ProductsPerPage)
                .ToListAsync(cancellationToken),
            Categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(category => category.Name)
                .ToListAsync(cancellationToken),
            SearchTerm = searchTerm,
            CategoryId = categoryId,
            Title = selectedCategory?.Name ?? "All Products",
            CurrentPage = page,
            TotalPages = totalPages,
            TotalItems = totalItems
        };

        return View(viewModel);
    }

    public IActionResult AllProducts() => RedirectToAction(nameof(Index));

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Where(item => item.IsActive)
            .Include(item => item.Category)
            .Include(item => item.Reviews)
                .ThenInclude(review => review.User)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return product is null ? NotFound() : View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequireCustomer]
    public async Task<IActionResult> AddReview(ReviewInputViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ReviewMessage"] = $"Rating must be between 1 and 5 and the comment cannot exceed {StoreSettings.MaximumReviewLength} characters.";
            return RedirectToAction(nameof(Details), new { id = model.ProductId });
        }

        var productExists = await _context.Products
            .AsNoTracking()
            .AnyAsync(product => product.Id == model.ProductId && product.IsActive, cancellationToken);
        if (!productExists)
        {
            return NotFound();
        }

        var userId = HttpContext.Session.GetCurrentUserId()!.Value;
        var review = await _context.Reviews
            .FirstOrDefaultAsync(item => item.UserId == userId && item.ProductId == model.ProductId, cancellationToken);

        if (review is null)
        {
            review = new Reviews
            {
                ProductId = model.ProductId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Reviews.Add(review);
        }

        review.Rating = model.Rating;
        review.Comment = model.Comment?.Trim() ?? string.Empty;
        review.CreatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            TempData["ReviewMessage"] = "Your review has been saved.";
        }
        catch (DbUpdateException exception)
        {
            _logger.LogWarning(exception, "Review save conflicted for user {UserId} and product {ProductId}.", userId, model.ProductId);
            TempData["ReviewMessage"] = "Your review could not be saved because another update was processed. Please try again.";
        }

        return RedirectToAction(nameof(Details), new { id = model.ProductId });
    }
}
