using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Filters;

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

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var orders = _context.Orders
                .Include(order => order.OrderItems)
                    .ThenInclude(item => item.Product)
                .Where(order => order.UserId == userId)
                .OrderByDescending(order => order.OrderDate)
                .ToList();

            return View(orders);
        }

        public IActionResult Details(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var order = _context.Orders
                .Include(item => item.User)
                .Include(item => item.OrderItems)
                    .ThenInclude(item => item.Product)
                .FirstOrDefault(item => item.Id == id && item.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
