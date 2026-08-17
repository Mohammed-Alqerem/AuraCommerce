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

        public IActionResult Index()
        {
            var viewModel = new StoreHomeViewModel
            {
                FeaturedProducts = _context.Products
                    .Include(product => product.Category)
                    .Include(product => product.Reviews)
                    .OrderBy(product => product.Id)
                    .Take(8)
                    .ToList(),
                Categories = _context.Categories
                    .Include(category => category.Products)
                    .OrderBy(category => category.Name)
                    .ToList(),
                ProductCount = _context.Products.Count(),
                OrderCount = _context.Orders.Count()
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
