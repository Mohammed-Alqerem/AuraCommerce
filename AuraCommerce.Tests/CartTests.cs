using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OnlineStore.Constants;
using OnlineStore.Controllers;
using OnlineStore.Extensions;
using OnlineStore.Models;

namespace AuraCommerce.Tests;

public class CartTests
{
    [Fact]
    public async Task Add_CreatesCartItem_AndCapsQuantityAtStock()
    {
        await using var database = await TestDatabase.CreateAsync();
        var user = CreateUser(100, "cart@example.com");
        database.Context.Users.Add(user);
        database.Context.Products.Add(new Products
        {
            Id = 100,
            Name = "Test Product",
            Description = "A product used by cart tests.",
            Price = 10m,
            Stock = 3,
            ImageUrl = "https://example.com/product.jpg",
            CategoryId = 1
        });
        await database.Context.SaveChangesAsync();
        var controller = CreateController(database, user.Id);

        var result = await controller.Add(100, 99);

        Assert.IsType<RedirectToActionResult>(result);
        var item = await database.Context.CartItems.SingleAsync(item => item.ProductId == 100);
        Assert.Equal(3, item.Quantity);
    }

    [Fact]
    public async Task Update_CannotModifyAnotherUsersCartItem()
    {
        await using var database = await TestDatabase.CreateAsync();
        var owner = CreateUser(101, "owner@example.com");
        var attacker = CreateUser(102, "attacker@example.com");
        var cart = new Cart { Id = 100, UserId = owner.Id, CreatedAt = DateTime.UtcNow };
        database.Context.AddRange(owner, attacker, cart);
        database.Context.CartItems.Add(new CartItems { Id = 100, CartId = cart.Id, ProductId = 1, Quantity = 1 });
        await database.Context.SaveChangesAsync();
        var controller = CreateController(database, attacker.Id);

        var result = await controller.Update(100, 2, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
        Assert.Equal(1, (await database.Context.CartItems.FindAsync(100))!.Quantity);
    }

    private static CartController CreateController(TestDatabase database, int userId)
    {
        var session = new TestSession();
        session.SetInt32(SessionKeys.UserId, userId);
        session.SetString(SessionKeys.UserRole, UserRoles.Customer);
        var httpContext = TestHttpContext.WithSession(session);
        return new CartController(database.Context, NullLogger<CartController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = new TempDataDictionary(httpContext, new TestTempDataProvider())
        };
    }

    private static Users CreateUser(int id, string email) => new()
    {
        Id = id,
        Name = "Test User",
        Email = email,
        NormalizedEmail = email.ToUpperInvariant(),
        Password = "not-used-in-this-test",
        Role = UserRoles.Customer,
        CreatedAt = DateTime.UtcNow
    };

    private sealed class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) =>
            new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}
