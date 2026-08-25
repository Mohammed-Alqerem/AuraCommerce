using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Extensions;
using OnlineStore.Filters;

namespace OnlineStore.Controllers;

[RequireCustomer]
public class NotificationsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetCurrentUserId()!.Value;
        return View(await context.StoreNotifications.AsNoTracking().Where(item => item.UserId == userId)
            .OrderByDescending(item => item.CreatedAt).Take(100).ToListAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> MarkRead(int id, CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetCurrentUserId()!.Value;
        var notification = await context.StoreNotifications.FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);
        if (notification is null) return NotFound();
        notification.IsRead = true;
        await context.SaveChangesAsync(cancellationToken);
        return !string.IsNullOrWhiteSpace(notification.Link) && Url.IsLocalUrl(notification.Link)
            ? LocalRedirect(notification.Link) : RedirectToAction(nameof(Index));
    }
}
