using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Filters;
using OnlineStore.Models;
using OnlineStore.Models.ViewModels;

namespace OnlineStore.Controllers;

[RequireAdmin]
[Route("Admin/Categories")]
public class AdminCategoriesController(ApplicationDbContext context) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await context.Categories.AsNoTracking().Include(category => category.Products)
            .OrderByDescending(category => category.IsActive).ThenBy(category => category.Name)
            .ToListAsync(cancellationToken));

    [HttpGet("Edit/{id:int?}")]
    public async Task<IActionResult> Edit(int? id, CancellationToken cancellationToken)
    {
        if (!id.HasValue) return View(new CategoryFormViewModel());
        var category = await context.Categories.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id.Value, cancellationToken);
        return category is null ? NotFound() : View(new CategoryFormViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive
        });
    }

    [HttpPost("Edit/{id:int?}")]
    public async Task<IActionResult> Edit(CategoryFormViewModel model, CancellationToken cancellationToken)
    {
        var normalizedName = model.Name.Trim();
        if (await context.Categories.AnyAsync(category => category.Name == normalizedName && category.Id != model.Id, cancellationToken))
            ModelState.AddModelError(nameof(model.Name), "A category with this name already exists.");
        if (!ModelState.IsValid) return View(model);

        var category = model.Id == 0 ? new Categories() : await context.Categories
            .FirstOrDefaultAsync(item => item.Id == model.Id, cancellationToken);
        if (category is null) return NotFound();
        if (model.Id == 0) context.Categories.Add(category);
        category.Name = normalizedName;
        category.Description = model.Description.Trim();
        category.IsActive = model.IsActive;
        await context.SaveChangesAsync(cancellationToken);
        TempData["AdminMessage"] = $"{category.Name} was saved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Toggle/{id:int}")]
    public async Task<IActionResult> Toggle(int id, CancellationToken cancellationToken)
    {
        var category = await context.Categories.Include(item => item.Products)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (category is null) return NotFound();
        if (category.IsActive && category.Products.Any(product => product.IsActive))
        {
            TempData["AdminMessage"] = "Archive or move the active products before archiving this category.";
            return RedirectToAction(nameof(Index));
        }
        category.IsActive = !category.IsActive;
        await context.SaveChangesAsync(cancellationToken);
        TempData["AdminMessage"] = category.IsActive ? "Category restored." : "Category archived.";
        return RedirectToAction(nameof(Index));
    }
}
