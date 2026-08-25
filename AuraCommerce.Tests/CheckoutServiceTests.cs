using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OnlineStore.Constants;
using OnlineStore.Models;
using OnlineStore.Services;
using OnlineStore.Models.ViewModels;

namespace AuraCommerce.Tests;

public class CheckoutServiceTests
{
    [Fact]
    public async Task Checkout_UsesDatabasePrice_ReducesStock_ClearsCart_AndCreatesOwnedOrder()
    {
        await using var database = await TestDatabase.CreateAsync();
        var (user, product, cart) = await AddCheckoutDataAsync(database, 200, "checkout@example.com", 20m, 5, 2);
        var service = new CheckoutService(database.Context, NullLogger<CheckoutService>.Instance);

        var result = await service.CheckoutAsync(user.Id, new CheckoutViewModel
        {
            FullName = "Delivery Name",
            Email = "delivery@example.com",
            Phone = "12345",
            Address = "1 Test Street"
        });

        Assert.True(result.Succeeded);
        var order = await database.Context.Orders
            .Include(item => item.OrderItems)
            .SingleAsync(item => item.Id == result.OrderId);
        Assert.Equal(user.Id, order.UserId);
        Assert.Equal(50.80m, order.TotalPrice);
        Assert.Equal(40m, order.Subtotal);
        Assert.Equal(8m, order.ShippingAmount);
        Assert.Equal(2.80m, order.TaxAmount);
        Assert.Equal("Delivery Name", order.ShippingName);
        Assert.Equal("1 Test Street", order.ShippingAddress);
        Assert.Single(await database.Context.OrderStatusHistory.Where(item => item.OrderId == order.Id).ToListAsync());
        Assert.Equal("Checkout Product", order.OrderItems.Single().ProductName);
        Assert.Equal(20m, order.OrderItems.Single().UnitPrice);
        Assert.Equal(3, (await database.Context.Products.FindAsync(product.Id))!.Stock);
        Assert.False(await database.Context.CartItems.AnyAsync(item => item.CartId == cart.Id));
    }

    [Fact]
    public async Task Checkout_RejectsEmptyCart()
    {
        await using var database = await TestDatabase.CreateAsync();
        var user = CreateUser(201, "empty@example.com");
        database.Context.Users.Add(user);
        await database.Context.SaveChangesAsync();
        var service = new CheckoutService(database.Context, NullLogger<CheckoutService>.Instance);

        var result = await service.CheckoutAsync(user.Id);

        Assert.False(result.Succeeded);
        Assert.Equal("Your cart is empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task Checkout_RejectsInsufficientStock_WithoutReducingStock()
    {
        await using var database = await TestDatabase.CreateAsync();
        var (user, product, _) = await AddCheckoutDataAsync(database, 202, "stock@example.com", 15m, 1, 2);
        var service = new CheckoutService(database.Context, NullLogger<CheckoutService>.Instance);

        var result = await service.CheckoutAsync(user.Id);

        Assert.False(result.Succeeded);
        Assert.Equal(1, (await database.Context.Products.FindAsync(product.Id))!.Stock);
        Assert.False(await database.Context.Orders.AnyAsync(item => item.UserId == user.Id));
    }

    [Fact]
    public async Task Checkout_AfterStockConsumed_CannotCreateSecondOrderOrNegativeStock()
    {
        await using var database = await TestDatabase.CreateAsync();
        var firstUser = CreateUser(203, "first@example.com");
        var secondUser = CreateUser(204, "second@example.com");
        var product = new Products
        {
            Id = 204,
            Name = "Last Item",
            Description = "Only one unit remains in stock.",
            Price = 30m,
            Stock = 1,
            ImageUrl = "https://example.com/last.jpg",
            CategoryId = 1
        };
        var firstCart = new Cart { Id = 203, UserId = firstUser.Id, CreatedAt = DateTime.UtcNow };
        var secondCart = new Cart { Id = 204, UserId = secondUser.Id, CreatedAt = DateTime.UtcNow };
        database.Context.AddRange(firstUser, secondUser, product, firstCart, secondCart);
        database.Context.CartItems.AddRange(
            new CartItems { Id = 203, CartId = firstCart.Id, ProductId = product.Id, Quantity = 1 },
            new CartItems { Id = 204, CartId = secondCart.Id, ProductId = product.Id, Quantity = 1 });
        await database.Context.SaveChangesAsync();
        var service = new CheckoutService(database.Context, NullLogger<CheckoutService>.Instance);

        var first = await service.CheckoutAsync(firstUser.Id);
        var second = await service.CheckoutAsync(secondUser.Id);

        Assert.True(first.Succeeded);
        Assert.False(second.Succeeded);
        Assert.Equal(0, (await database.Context.Products.FindAsync(product.Id))!.Stock);
        Assert.Equal(1, await database.Context.Orders.CountAsync(item =>
            item.UserId == firstUser.Id || item.UserId == secondUser.Id));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Checkout_RejectsDeliveryFieldsLongerThanDatabaseColumns(bool overlongEmail)
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new CheckoutService(database.Context, NullLogger<CheckoutService>.Instance);
        var checkout = new CheckoutViewModel
        {
            FullName = "Delivery Name",
            Email = overlongEmail ? $"{new string('a', 245)}@example.com" : "delivery@example.com",
            Phone = overlongEmail ? "12345" : new string('1', 31),
            Address = "1 Test Street"
        };

        var result = await service.CheckoutAsync(2, checkout);

        Assert.False(result.Succeeded);
        Assert.False(await database.Context.Orders.AnyAsync(order => order.UserId == 2 && order.Id > 4));
    }

    private static async Task<(Users User, Products Product, Cart Cart)> AddCheckoutDataAsync(
        TestDatabase database,
        int id,
        string email,
        decimal price,
        int stock,
        int quantity)
    {
        var user = CreateUser(id, email);
        var product = new Products
        {
            Id = id,
            Name = "Checkout Product",
            Description = "A product used by checkout tests.",
            Price = price,
            Stock = stock,
            ImageUrl = "https://example.com/checkout.jpg",
            CategoryId = 1
        };
        var cart = new Cart { Id = id, UserId = user.Id, CreatedAt = DateTime.UtcNow };
        database.Context.AddRange(user, product, cart);
        database.Context.CartItems.Add(new CartItems
        {
            Id = id,
            CartId = cart.Id,
            ProductId = product.Id,
            Quantity = quantity
        });
        await database.Context.SaveChangesAsync();
        return (user, product, cart);
    }

    private static Users CreateUser(int id, string email) => new()
    {
        Id = id,
        Name = "Checkout User",
        Email = email,
        NormalizedEmail = email.ToUpperInvariant(),
        Password = "not-used-in-this-test",
        Role = UserRoles.Customer,
        Phone = "12345",
        Address = "1 Test Street",
        CreatedAt = DateTime.UtcNow
    };
}
