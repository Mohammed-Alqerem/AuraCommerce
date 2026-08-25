using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Filters;
using OnlineStore.Extensions;

namespace OnlineStore.Controllers
{
    [RequireCustomer]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var userId = HttpContext.Session.GetCurrentUserId()!.Value;
            var orders = await _context.Orders
                .AsNoTracking()
                .Include(order => order.OrderItems)
                    .ThenInclude(item => item.Product)
                .Where(order => order.UserId == userId)
                .OrderByDescending(order => order.OrderDate)
                .ToListAsync(cancellationToken);

            return View(orders);
        }

        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var userId = HttpContext.Session.GetCurrentUserId()!.Value;
            var order = await _context.Orders
                .AsNoTracking()
                .Include(item => item.User)
                .Include(item => item.OrderItems)
                    .ThenInclude(item => item.Product)
                .Include(item => item.StatusHistory.OrderBy(history => history.CreatedAt))
                .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
