using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Filters;
using OnlineStore.Models;
using OnlineStore.Models.ViewModels;

namespace OnlineStore.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string? searchTerm, int? categoryId)
        {
            var productsQuery = _context.Products
                .Include(product => product.Category)
                .Include(product => product.Reviews)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                productsQuery = productsQuery.Where(product =>
                    product.Name.Contains(searchTerm) ||
                    product.Description.Contains(searchTerm) ||
                    (product.Category != null && product.Category.Name.Contains(searchTerm)));
            }

            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(product => product.CategoryId == categoryId.Value);
            }

            var selectedCategory = categoryId.HasValue
                ? _context.Categories.FirstOrDefault(category => category.Id == categoryId.Value)
                : null;

            var viewModel = new ProductCatalogViewModel
            {
                Products = productsQuery.OrderBy(product => product.Name).ToList(),
                Categories = _context.Categories.OrderBy(category => category.Name).ToList(),
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                Title = selectedCategory != null ? selectedCategory.Name : "All Products"
            };

            return View(viewModel);
        }

        public IActionResult AllProducts()
        {
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            var product = _context.Products
                .Include(product => product.Category)
                .Include(product => product.Reviews)
                    .ThenInclude(review => review.User)
                .FirstOrDefault(product => product.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireCustomer]
        public IActionResult AddReview(int productId, int rating, string? comment)
        {
            if (rating >= 1 && rating <= 5)
            {
                _context.Reviews.Add(new Reviews
                {
                    ProductId = productId,
                    UserId = GetCurrentUserId(),
                    Rating = rating,
                    Comment = comment ?? string.Empty,
                    CreatedAt = DateTime.Now
                });
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Details), new { id = productId });
        }

        private int GetCurrentUserId()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                throw new InvalidOperationException("A user session is required to add reviews.");
            }

            return userId.Value;
        }
    }
}
