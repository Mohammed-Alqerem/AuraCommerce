using System.Data;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Constants;
using OnlineStore.Data;
using OnlineStore.Models;
using OnlineStore.Models.ViewModels;

namespace OnlineStore.Services;

public interface ICheckoutService
{
    Task<CartViewModel> GetCartAsync(int userId, CancellationToken cancellationToken = default);
    Task<CheckoutResult> CheckoutAsync(int userId, CancellationToken cancellationToken = default);
    Task<CheckoutResult> CheckoutAsync(int userId, CheckoutViewModel checkout, CancellationToken cancellationToken = default);
}

public sealed record CheckoutResult(bool Succeeded, int? OrderId = null, string? ErrorMessage = null)
{
    public static CheckoutResult Success(int orderId) => new(true, orderId);
    public static CheckoutResult Failure(string message) => new(false, ErrorMessage: message);
}

public sealed class CheckoutService : ICheckoutService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CheckoutService> _logger;

    public CheckoutService(ApplicationDbContext context, ILogger<CheckoutService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CartViewModel> GetCartAsync(int userId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var items = await _context.CartItems
            .AsNoTracking()
            .Include(item => item.Product)
            .Where(item => item.CartId == cart.Id)
            .OrderBy(item => item.Id)
            .ToListAsync(cancellationToken);

        return new CartViewModel { Cart = cart, Items = items };
    }

    public async Task<CheckoutResult> CheckoutAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null)
        {
            return CheckoutResult.Failure("Your account could not be found.");
        }

        return await CheckoutAsync(userId, new CheckoutViewModel
        {
            FullName = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address
        }, cancellationToken);
    }

    public async Task<CheckoutResult> CheckoutAsync(
        int userId,
        CheckoutViewModel checkout,
        CancellationToken cancellationToken = default)
    {
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(checkout, new ValidationContext(checkout), validationResults, validateAllProperties: true))
        {
            return CheckoutResult.Failure(validationResults.FirstOrDefault()?.ErrorMessage ?? "Enter valid delivery details.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            var cart = await GetOrCreateCartAsync(userId, cancellationToken);
            var items = await _context.CartItems
                .Include(item => item.Product)
                .Where(item => item.CartId == cart.Id)
                .OrderBy(item => item.Id)
                .ToListAsync(cancellationToken);

            if (items.Count == 0)
            {
                return CheckoutResult.Failure("Your cart is empty.");
            }

            var unavailableItems = items
                .Where(item => item.Product is null || !item.Product.IsActive || item.Quantity < 1 || item.Quantity > item.Product.Stock)
                .Select(item => item.Product?.Name ?? "A product")
                .Distinct()
                .ToList();

            if (unavailableItems.Count > 0)
            {
                return CheckoutResult.Failure($"Insufficient stock or unavailable product: {string.Join(", ", unavailableItems)}.");
            }

            var cartViewModel = new CartViewModel { Cart = cart, Items = items };
            var order = new Orders
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Subtotal = cartViewModel.Subtotal,
                ShippingAmount = cartViewModel.Shipping,
                TaxAmount = cartViewModel.Tax,
                TotalPrice = cartViewModel.Total,
                Status = OrderStatuses.Processing,
                ShippingName = checkout.FullName.Trim(),
                ShippingEmail = checkout.Email.Trim(),
                ShippingPhone = checkout.Phone.Trim(),
                ShippingAddress = checkout.Address.Trim(),
                DeliveryMethod = "Standard"
            };

            order.StatusHistory.Add(new OrderStatusHistory
            {
                Status = order.Status,
                Note = "Order placed"
            });

            foreach (var cartItem in items)
            {
                var product = cartItem.Product!;
                order.OrderItems.Add(new OrderItems
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = cartItem.Quantity,
                    UnitPrice = product.Price
                });
                product.Stock -= cartItem.Quantity;
            }

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Checkout created order {OrderId} for user {UserId}.", order.Id, userId);
            return CheckoutResult.Success(order.Id);
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(exception, "Checkout failed for user {UserId}.", userId);
            throw;
        }
    }

    private async Task<Cart> GetOrCreateCartAsync(int userId, CancellationToken cancellationToken)
    {
        var cart = await _context.Carts.FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken);
        if (cart is not null)
        {
            return cart;
        }

        cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync(cancellationToken);
        return cart;
    }
}
