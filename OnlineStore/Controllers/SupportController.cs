using Microsoft.AspNetCore.Mvc;
using OnlineStore.Data;
using OnlineStore.Extensions;
using OnlineStore.Models;
using OnlineStore.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Filters;

namespace OnlineStore.Controllers;

public class SupportController(ApplicationDbContext context) : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new SupportTicketViewModel());

    [HttpPost]
    public async Task<IActionResult> Index(SupportTicketViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        context.SupportTickets.Add(new SupportTicket
        {
            UserId = HttpContext.Session.GetCurrentUserId(),
            Name = model.Name.Trim(),
            Email = model.Email.Trim(),
            Subject = model.Subject.Trim(),
            Message = model.Message.Trim()
        });
        await context.SaveChangesAsync(cancellationToken);
        TempData["StoreMessage"] = "Your support request was received.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Faq() => View();
    public IActionResult Shipping() => View();
    public IActionResult Returns() => View();
    public IActionResult Terms() => View();
    public IActionResult About() => View();

    [RequireCustomer]
    public async Task<IActionResult> MyTickets(CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetCurrentUserId()!.Value;
        return View(await context.SupportTickets.AsNoTracking().Where(ticket => ticket.UserId == userId)
            .OrderByDescending(ticket => ticket.CreatedAt).ToListAsync(cancellationToken));
    }
}
