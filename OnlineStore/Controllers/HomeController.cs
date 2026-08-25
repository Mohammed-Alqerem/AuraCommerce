using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Models;
using OnlineStore.Models.ViewModels;
using System.Diagnostics;

namespace OnlineStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var featuredProducts = await _context.Products
                .AsNoTracking()
                .Where(product => product.IsActive)
                .Include(product => product.Category)
                .Include(product => product.Reviews.Where(review => review.IsVisible))
                .OrderByDescending(product => product.IsFeatured)
                .ThenByDescending(product => product.CreatedAt)
                .Take(8)
                .ToListAsync(cancellationToken);
            var categories = await _context.Categories
                .AsNoTracking()
                .Where(category => category.IsActive)
                .Include(category => category.Products.Where(product => product.IsActive))
                .OrderBy(category => category.Name)
                .ToListAsync(cancellationToken);

            var viewModel = new StoreHomeViewModel
            {
                FeaturedProducts = featuredProducts,
                Categories = categories,
                ProductCount = await _context.Products.CountAsync(product => product.IsActive, cancellationToken),
                OrderCount = await _context.Orders.CountAsync(cancellationToken)
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
