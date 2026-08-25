using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Filters;
using OnlineStore.Models;
using OnlineStore.Models.ViewModels;

namespace OnlineStore.Controllers;

[RequireAdmin]
[Route("Admin/Inventory")]
public class AdminInventoryController(ApplicationDbContext context) : Controller
{
    private const int PageSize = 20;

    [HttpGet("")]
    public async Task<IActionResult> Index(string? search, string stockState = "all", int page = 1, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        var query = context.Products.AsNoTracking().Include(product => product.Category).AsQueryable();
        if (search is not null) query = query.Where(product => product.Name.Contains(search) || product.Sku.Contains(search));
        query = stockState switch
        {
            "out" => query.Where(product => product.Stock == 0),
            "low" => query.Where(product => product.Stock > 0 && product.Stock <= product.LowStockThreshold),
            "healthy" => query.Where(product => product.Stock > product.LowStockThreshold),
            _ => query
        };
        var total = await query.CountAsync(cancellationToken);
        var pages = (int)Math.Ceiling(total / (double)PageSize);
        if (pages > 0) page = Math.Min(page, pages);
        return View(new InventoryViewModel
        {
            Products = await query.OrderBy(product => product.Stock).ThenBy(product => product.Name)
                .Skip((page - 1) * PageSize).Take(PageSize).ToListAsync(cancellationToken),
            Search = search,
            StockState = stockState,
            Page = page,
            TotalPages = pages,
            TotalItems = total
        });
    }

    [HttpPost("Adjust")]
    public async Task<IActionResult> Adjust(InventoryAdjustmentViewModel model, CancellationToken cancellationToken)
    {
        if (model.QuantityChange == 0) ModelState.AddModelError(nameof(model.QuantityChange), "Enter a non-zero adjustment.");
        var product = await context.Products.AsNoTracking()
            .Where(item => item.Id == model.ProductId)
            .Select(item => new { item.Id, item.Name })
            .FirstOrDefaultAsync(cancellationToken);
        if (product is null) return NotFound();
        if (!ModelState.IsValid)
        {
            TempData["AdminMessage"] = string.Join(" ", ModelState.Values.SelectMany(value => value.Errors).Select(error => error.ErrorMessage));
            return RedirectToAction(nameof(Index));
        }

        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var maximumStockBeforeAdjustment = 100000 - model.QuantityChange;
        var eligibleProducts = context.Products.Where(item =>
            item.Id == product.Id && item.Stock <= maximumStockBeforeAdjustment);
        if (model.QuantityChange < 0)
        {
            eligibleProducts = eligibleProducts.Where(item => item.Stock >= -model.QuantityChange);
        }
        var affectedRows = await eligibleProducts
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.Stock, item => item.Stock + model.QuantityChange), cancellationToken);
        if (affectedRows == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            TempData["AdminMessage"] = "Stock must remain between 0 and 100,000.";
            return RedirectToAction(nameof(Index));
        }

        var stockAfter = await context.Products.AsNoTracking()
            .Where(item => item.Id == product.Id)
            .Select(item => item.Stock)
            .SingleAsync(cancellationToken);
        context.InventoryAdjustments.Add(new InventoryAdjustment
        {
            ProductId = product.Id,
            QuantityChange = model.QuantityChange,
            StockAfter = stockAfter,
            Reason = model.Reason.Trim()
        });
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        TempData["AdminMessage"] = $"Stock for {product.Name} is now {stockAfter}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("History/{productId:int}")]
    public async Task<IActionResult> History(int productId, CancellationToken cancellationToken)
    {
        var product = await context.Products.AsNoTracking().FirstOrDefaultAsync(item => item.Id == productId, cancellationToken);
        if (product is null) return NotFound();
        ViewBag.Product = product;
        return View(await context.InventoryAdjustments.AsNoTracking().Where(item => item.ProductId == productId)
            .OrderByDescending(item => item.CreatedAt).Take(200).ToListAsync(cancellationToken));
    }
}
