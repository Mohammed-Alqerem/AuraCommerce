using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Filters;

namespace OnlineStore.Controllers;

[RequireAdmin]
[Route("Admin/Reviews")]
public class AdminReviewsController(ApplicationDbContext context) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) => View(
        await context.Reviews.AsNoTracking().Include(review => review.User).Include(review => review.Product)
            .OrderByDescending(review => review.CreatedAt).Take(250).ToListAsync(cancellationToken));

    [HttpPost("Toggle/{id:int}")]
    public async Task<IActionResult> Toggle(int id, CancellationToken cancellationToken)
    {
        var review = await context.Reviews.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (review is null) return NotFound();
        review.IsVisible = !review.IsVisible;
        await context.SaveChangesAsync(cancellationToken);
        TempData["AdminMessage"] = review.IsVisible ? "Review restored." : "Review hidden from the storefront.";
        return RedirectToAction(nameof(Index));
    }
}
