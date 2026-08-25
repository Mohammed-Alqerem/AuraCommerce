using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Filters;

namespace OnlineStore.Controllers;

[RequireAdmin]
[Route("Admin/Support")]
public class AdminSupportController(ApplicationDbContext context) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) => View(
        await context.SupportTickets.AsNoTracking().Include(ticket => ticket.User)
            .OrderBy(ticket => ticket.Status == "Open" ? 0 : 1).ThenByDescending(ticket => ticket.CreatedAt)
            .Take(250).ToListAsync(cancellationToken));

    [HttpPost("Status/{id:int}")]
    public async Task<IActionResult> Status(int id, string status, CancellationToken cancellationToken)
    {
        if (status is not ("Open" or "Resolved")) return BadRequest();
        var ticket = await context.SupportTickets.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (ticket is null) return NotFound();
        ticket.Status = status;
        await context.SaveChangesAsync(cancellationToken);
        TempData["AdminMessage"] = $"Support request #{ticket.Id} is now {status.ToLowerInvariant()}.";
        return RedirectToAction(nameof(Index));
    }
}
