using System.ComponentModel.DataAnnotations;
using ClosedXML.Excel;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OnlineStore.Constants;
using OnlineStore.Controllers;
using OnlineStore.Models;
using OnlineStore.Models.ViewModels;
using OnlineStore.Services;

namespace AuraCommerce.Tests;

public class StoreExpansionTests
{
    [Fact]
    public void AccountTokens_RejectMissingAndMalformedValues()
    {
        var service = new AccountTokenService(new EphemeralDataProtectionProvider());

        Assert.False(service.TryRead(string.Empty, "password-reset", out _));
        Assert.False(service.TryRead("not-a-protected-token", "password-reset", out _));
    }

    [Fact]
    public async Task Wishlist_RejectsDuplicateCustomerProduct()
    {
        await using var database = await TestDatabase.CreateAsync();
        database.Context.WishlistItems.AddRange(
            new WishlistItem { UserId = 2, ProductId = 1 },
            new WishlistItem { UserId = 2, ProductId = 1 });

        await Assert.ThrowsAsync<DbUpdateException>(() => database.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task OrderStatusHistory_IsRemovedWithOrder()
    {
        await using var database = await TestDatabase.CreateAsync();
        var order = new Orders
        {
            UserId = 2,
            OrderDate = DateTime.UtcNow,
            Subtotal = 10,
            TotalPrice = 10,
            Status = OrderStatuses.Processing,
            ShippingName = "Test",
            ShippingEmail = "test@example.com",
            ShippingAddress = "Test address",
            DeliveryMethod = "Standard"
        };
        order.StatusHistory.Add(new OrderStatusHistory { Status = OrderStatuses.Processing, Note = "Placed" });
        database.Context.Orders.Add(order);
        await database.Context.SaveChangesAsync();

        database.Context.Orders.Remove(order);
        await database.Context.SaveChangesAsync();

        Assert.False(await database.Context.OrderStatusHistory.AnyAsync(item => item.OrderId == order.Id));
    }

    [Fact]
    public async Task InventoryAdjustment_PreservesAuditValues()
    {
        await using var database = await TestDatabase.CreateAsync();
        var product = await database.Context.Products.FirstAsync(item => item.Id == 1);
        var initialStock = product.Stock;
        database.Context.Entry(product).State = EntityState.Detached;
        var controller = new AdminInventoryController(database.Context);
        TestHttpContext.AttachTo(controller);

        var result = await controller.Adjust(new InventoryAdjustmentViewModel
        {
            ProductId = product.Id,
            QuantityChange = 5,
            Reason = "Delivery received"
        }, CancellationToken.None);

        Assert.IsType<RedirectToActionResult>(result);
        var adjustment = await database.Context.InventoryAdjustments.AsNoTracking().SingleAsync();
        Assert.Equal(5, adjustment.QuantityChange);
        Assert.Equal(initialStock + 5, adjustment.StockAfter);
        Assert.Equal(initialStock + 5, await database.Context.Products.Where(item => item.Id == product.Id).Select(item => item.Stock).SingleAsync());
        Assert.Equal("Delivery received", adjustment.Reason);
    }

    [Fact]
    public async Task InventoryAdjustment_RejectsOutOfRangeResultWithoutAuditRow()
    {
        await using var database = await TestDatabase.CreateAsync();
        var product = await database.Context.Products.AsNoTracking().FirstAsync(item => item.Id == 1);
        var controller = new AdminInventoryController(database.Context);
        TestHttpContext.AttachTo(controller);

        var result = await controller.Adjust(new InventoryAdjustmentViewModel
        {
            ProductId = product.Id,
            QuantityChange = -(product.Stock + 1),
            Reason = "Invalid correction"
        }, CancellationToken.None);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(product.Stock, await database.Context.Products.Where(item => item.Id == product.Id).Select(item => item.Stock).SingleAsync());
        Assert.False(await database.Context.InventoryAdjustments.AnyAsync(item => item.ProductId == product.Id));
    }

    [Fact]
    public void ProductForm_AllowsBlankOptionalMediaAndMerchandisingFields()
    {
        var model = new ProductFormViewModel
        {
            Name = "Optional fields product",
            Description = "A valid product without optional media or merchandising fields.",
            Price = 10,
            Stock = 1,
            CategoryId = 1,
            ImageUrl = null,
            Sku = null,
            Brand = null,
            AdditionalImageUrls = null
        };
        var results = new List<ValidationResult>();

        var valid = Validator.TryValidateObject(model, new ValidationContext(model), results, true);

        Assert.True(valid, string.Join("; ", results.Select(result => result.ErrorMessage)));
    }

    [Fact]
    public async Task ProductCatalog_RatingSortIgnoresHiddenReviews()
    {
        await using var database = await TestDatabase.CreateAsync();
        var category = new Categories { Id = 99, Name = "Moderation test", Description = "Test category" };
        var inflated = new Products
        {
            Id = 501,
            Name = "Inflated",
            Description = "A product with a hidden high rating.",
            Price = 10,
            Stock = 1,
            CategoryId = category.Id
        };
        var honest = new Products
        {
            Id = 502,
            Name = "Honest",
            Description = "A product with a visible higher rating.",
            Price = 10,
            Stock = 1,
            CategoryId = category.Id
        };
        database.Context.AddRange(category, inflated, honest);
        database.Context.Reviews.AddRange(
            new Reviews { Id = 501, UserId = 2, ProductId = inflated.Id, Rating = 1, Comment = "Visible", IsVisible = true },
            new Reviews { Id = 502, UserId = 3, ProductId = inflated.Id, Rating = 5, Comment = "Hidden", IsVisible = false },
            new Reviews { Id = 503, UserId = 2, ProductId = honest.Id, Rating = 2, Comment = "Visible", IsVisible = true });
        await database.Context.SaveChangesAsync();
        var controller = new ProductsController(database.Context, NullLogger<ProductsController>.Instance);
        TestHttpContext.AttachTo(controller);

        var result = await controller.Index(null, category.Id, null, null, false, null, "rating");

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProductCatalogViewModel>(view.Model);
        Assert.Equal(new[] { honest.Id, inflated.Id }, model.Products.Select(product => product.Id));

        var filteredResult = await controller.Index(null, category.Id, null, null, false, 3, "name");
        var filteredView = Assert.IsType<ViewResult>(filteredResult);
        var filteredModel = Assert.IsType<ProductCatalogViewModel>(filteredView.Model);
        Assert.Empty(filteredModel.Products);
    }

    [Fact]
    public async Task ProductForm_RejectsDuplicateSkuWithoutSaving()
    {
        await using var database = await TestDatabase.CreateAsync();
        var controller = new AdminController(database.Context, NullLogger<AdminController>.Instance);
        TestHttpContext.AttachTo(controller);
        var productCount = await database.Context.Products.CountAsync();

        var result = await controller.ProductForm(new ProductFormViewModel
        {
            Name = "Duplicate SKU product",
            Description = "A valid product that duplicates an existing SKU.",
            Price = 10,
            Stock = 1,
            CategoryId = 1,
            Sku = " AURA-001 "
        }, CancellationToken.None);

        Assert.IsType<ViewResult>(result);
        Assert.True(controller.ModelState.ContainsKey(nameof(ProductFormViewModel.Sku)));
        Assert.Equal(productCount, await database.Context.Products.CountAsync());
    }

    [Fact]
    public async Task ProductForm_RejectsActiveProductInArchivedCategory()
    {
        await using var database = await TestDatabase.CreateAsync();
        var category = new Categories
        {
            Id = 97,
            Name = "Archived category",
            Description = "Not visible in the storefront",
            IsActive = false
        };
        database.Context.Categories.Add(category);
        await database.Context.SaveChangesAsync();
        var controller = new AdminController(database.Context, NullLogger<AdminController>.Instance);
        TestHttpContext.AttachTo(controller);

        var result = await controller.ProductForm(new ProductFormViewModel
        {
            Name = "Hidden category product",
            Description = "A valid active product submitted for an archived category.",
            Price = 10,
            Stock = 1,
            CategoryId = category.Id,
            IsActive = true
        }, CancellationToken.None);

        Assert.IsType<ViewResult>(result);
        Assert.True(controller.ModelState.ContainsKey(nameof(ProductFormViewModel.CategoryId)));
        Assert.False(await database.Context.Products.AnyAsync(item => item.Name == "Hidden category product"));
    }

    [Fact]
    public async Task SalesExport_CreatesStyledExcelWorkbookWithTypedValuesAndSafeText()
    {
        await using var database = await TestDatabase.CreateAsync();
        var order = new Orders
        {
            UserId = 2,
            OrderDate = DateTime.UtcNow,
            Subtotal = 21m,
            TotalPrice = 21m,
            Status = OrderStatuses.Processing,
            ShippingName = "Test",
            ShippingEmail = "test@example.com",
            ShippingAddress = "Test address",
            DeliveryMethod = "Standard"
        };
        order.OrderItems.Add(new OrderItems
        {
            ProductId = 1,
            ProductName = "=SUM(1,1)",
            Quantity = 2,
            UnitPrice = 10.5m
        });
        database.Context.Orders.Add(order);
        await database.Context.SaveChangesAsync();
        var controller = new AdminReportsController(database.Context, new SalesReportWorkbookExporter());
        TestHttpContext.AttachTo(controller);

        var result = await controller.Export(null, null, CancellationToken.None);

        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.ContentType);
        Assert.EndsWith(".xlsx", file.FileDownloadName, StringComparison.OrdinalIgnoreCase);
        Assert.Equal((byte)'P', file.FileContents[0]);
        Assert.Equal((byte)'K', file.FileContents[1]);

        using var stream = new MemoryStream(file.FileContents);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet("Sales report");
        var productNameCell = worksheet.CellsUsed()
            .Single(cell => cell.GetString() == "=SUM(1,1)");

        Assert.False(productNameCell.HasFormula);
        Assert.Equal(XLDataType.DateTime, worksheet.Cell("C4").DataType);
        Assert.Equal("$#,##0.00", worksheet.Cell("A7").Style.NumberFormat.Format);
        Assert.True(worksheet.Cell("H7").HasFormula);
        Assert.Contains(worksheet.Tables, table => table.Name == "BestSellers");
        Assert.Contains(worksheet.Tables, table => table.Name == "CategorySales");
    }

    [Fact]
    public async Task UpdatingOrderToCurrentStatus_DoesNotCreateHistoryOrNotification()
    {
        await using var database = await TestDatabase.CreateAsync();
        var order = await database.Context.Orders.AsNoTracking().FirstAsync();
        var historyCount = await database.Context.OrderStatusHistory.CountAsync();
        var notificationCount = await database.Context.StoreNotifications.CountAsync();
        var controller = new AdminController(database.Context, NullLogger<AdminController>.Instance);
        TestHttpContext.AttachTo(controller);

        var result = await controller.UpdateOrderStatus(order.Id, order.Status, CancellationToken.None);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(historyCount, await database.Context.OrderStatusHistory.CountAsync());
        Assert.Equal(notificationCount, await database.Context.StoreNotifications.CountAsync());
    }
}
