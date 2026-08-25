using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Constants;
using OnlineStore.Data;
using OnlineStore.Filters;
using OnlineStore.Models.ViewModels;
using OnlineStore.Services;

namespace OnlineStore.Controllers;

[RequireAdmin]
[Route("Admin/Reports")]
public class AdminReportsController(
    ApplicationDbContext context,
    ISalesReportWorkbookExporter workbookExporter) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(DateTime? from, DateTime? to, CancellationToken cancellationToken)
        => View(await BuildAsync(from, to, cancellationToken));

    [HttpGet("Export")]
    public async Task<IActionResult> Export(DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var report = await BuildAsync(from, to, cancellationToken);
        return File(
            workbookExporter.Export(report),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"aura-sales-{report.From:yyyyMMdd}-{report.To:yyyyMMdd}.xlsx");
    }

    private async Task<ReportsViewModel> BuildAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var end = (to ?? DateTime.UtcNow.Date).Date.AddDays(1);
        var start = (from ?? end.AddDays(-30)).Date;
        if (start >= end) start = end.AddDays(-30);
        var orders = context.Orders.AsNoTracking().Where(order => order.OrderDate >= start && order.OrderDate < end && order.Status != OrderStatuses.Cancelled);
        var count = await orders.CountAsync(cancellationToken);
        var revenue = await orders.SumAsync(order => (decimal?)order.TotalPrice, cancellationToken) ?? 0;
        var bestRows = await context.OrderItems.AsNoTracking()
            .Where(item => item.Order!.OrderDate >= start && item.Order.OrderDate < end && item.Order.Status != OrderStatuses.Cancelled)
            .GroupBy(item => new { item.ProductId, item.ProductName })
            .Select(group => new
            {
                group.Key.ProductId,
                group.Key.ProductName,
                Units = group.Sum(item => item.Quantity),
                Revenue = group.Sum(item => item.UnitPrice * item.Quantity)
            })
            .OrderByDescending(row => row.Units).Take(10).ToListAsync(cancellationToken);
        var best = bestRows
            .Select(row => new ProductSalesRow(row.ProductId, row.ProductName, row.Units, row.Revenue))
            .ToList();
        var categoryRows = await context.OrderItems.AsNoTracking()
            .Where(item => item.Order!.OrderDate >= start && item.Order.OrderDate < end && item.Order.Status != OrderStatuses.Cancelled)
            .GroupBy(item => item.Product!.Category!.Name)
            .Select(group => new
            {
                Name = group.Key,
                Units = group.Sum(item => item.Quantity),
                Revenue = group.Sum(item => item.UnitPrice * item.Quantity)
            })
            .ToListAsync(cancellationToken);
        var categories = categoryRows
            .OrderByDescending(row => row.Revenue)
            .Select(row => new CategorySalesRow(row.Name, row.Units, row.Revenue))
            .ToList();
        return new ReportsViewModel
        {
            From = start,
            To = end.AddDays(-1),
            Revenue = revenue,
            OrderCount = count,
            AverageOrderValue = count == 0 ? 0 : revenue / count,
            BestSellers = best,
            CategorySales = categories
        };
    }
}
