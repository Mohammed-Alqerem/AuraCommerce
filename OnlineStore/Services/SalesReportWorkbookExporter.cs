using ClosedXML.Excel;
using OnlineStore.Models.ViewModels;

namespace OnlineStore.Services;

public interface ISalesReportWorkbookExporter
{
    byte[] Export(ReportsViewModel report);
}

public sealed class SalesReportWorkbookExporter : ISalesReportWorkbookExporter
{
    private static readonly XLColor Navy = XLColor.FromHtml("#0A192F");
    private static readonly XLColor Accent = XLColor.FromHtml("#0062CC");
    private static readonly XLColor PaleBlue = XLColor.FromHtml("#EAF3FF");
    private static readonly XLColor SoftSurface = XLColor.FromHtml("#F3F5F8");
    private static readonly XLColor Muted = XLColor.FromHtml("#5F6673");
    private static readonly XLColor Line = XLColor.FromHtml("#D9E0E8");
    private static readonly XLColor White = XLColor.White;

    public byte[] Export(ReportsViewModel report)
    {
        ArgumentNullException.ThrowIfNull(report);

        using var workbook = new XLWorkbook();
        workbook.Properties.Title = "Aura Commerce sales report";
        workbook.Properties.Subject = $"Sales performance from {report.From:yyyy-MM-dd} to {report.To:yyyy-MM-dd}";
        workbook.Properties.Company = "Aura Commerce";

        var worksheet = workbook.Worksheets.Add("Sales report");
        ConfigureWorksheet(worksheet);
        WriteHeading(worksheet, report);
        WriteMetrics(worksheet, report);

        var bestSellersLastRow = WriteBestSellers(worksheet, report);
        var categoriesLastRow = WriteCategories(worksheet, report);
        var lastRow = Math.Max(bestSellersLastRow, categoriesLastRow);
        WriteFooter(worksheet, lastRow + 3);

        worksheet.SheetView.FreezeRows(11);
        worksheet.PageSetup.PrintAreas.Add($"A1:J{lastRow + 3}");

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void ConfigureWorksheet(IXLWorksheet worksheet)
    {
        worksheet.ShowGridLines = false;
        worksheet.TabColor = Accent;
        worksheet.Style.Font.FontName = "Aptos";
        worksheet.Style.Font.FontSize = 10;
        worksheet.Style.Font.FontColor = Navy;
        worksheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        worksheet.Column("A").Width = 7;
        worksheet.Column("B").Width = 31;
        worksheet.Column("C").Width = 12;
        worksheet.Column("D").Width = 16;
        worksheet.Column("E").Width = 13;
        worksheet.Column("F").Width = 16;
        worksheet.Column("G").Width = 3;
        worksheet.Column("H").Width = 25;
        worksheet.Column("I").Width = 12;
        worksheet.Column("J").Width = 16;

        worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
        worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
        worksheet.PageSetup.FitToPages(1, 1);
        worksheet.PageSetup.Margins.Top = 0.35;
        worksheet.PageSetup.Margins.Bottom = 0.35;
        worksheet.PageSetup.Margins.Left = 0.35;
        worksheet.PageSetup.Margins.Right = 0.35;
    }

    private static void WriteHeading(IXLWorksheet worksheet, ReportsViewModel report)
    {
        var title = worksheet.Range("A1:J2").Merge();
        title.Value = "AURA COMMERCE  /  SALES PERFORMANCE";
        title.Style.Fill.BackgroundColor = Navy;
        title.Style.Font.FontColor = White;
        title.Style.Font.FontSize = 22;
        title.Style.Font.Bold = true;
        title.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        worksheet.Rows(1, 2).Height = 24;

        var periodLabel = worksheet.Range("A4:B4").Merge();
        periodLabel.Value = "REPORTING PERIOD";
        periodLabel.Style.Font.Bold = true;
        periodLabel.Style.Font.FontColor = Accent;
        periodLabel.Style.Font.FontSize = 9;

        worksheet.Cell("C4").Value = report.From;
        worksheet.Cell("C4").Style.DateFormat.Format = "mmm d, yyyy";
        worksheet.Cell("D4").Value = "to";
        worksheet.Cell("D4").Style.Font.FontColor = Muted;
        worksheet.Cell("D4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        worksheet.Cell("E4").Value = report.To;
        worksheet.Cell("E4").Style.DateFormat.Format = "mmm d, yyyy";

        var note = worksheet.Range("H4:J4").Merge();
        note.Value = "Cancelled orders excluded";
        note.Style.Font.FontColor = Muted;
        note.Style.Font.Italic = true;
        note.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

        var rule = worksheet.Range("A5:J5");
        rule.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        rule.Style.Border.BottomBorderColor = Line;
    }

    private static void WriteMetrics(IXLWorksheet worksheet, ReportsViewModel report)
    {
        WriteMetric(worksheet, "A6:C6", "A7:C8", "REVENUE", report.Revenue, "$#,##0.00");
        WriteMetric(worksheet, "D6:F6", "D7:F8", "ORDERS", report.OrderCount, "#,##0");

        var averageLabel = worksheet.Range("H6:J6").Merge();
        var averageValue = worksheet.Range("H7:J8").Merge();
        StyleMetric(averageLabel, averageValue, "AVERAGE ORDER");
        worksheet.Cell("H7").FormulaA1 = "=IF(D7=0,0,A7/D7)";
        worksheet.Cell("H7").Style.NumberFormat.Format = "$#,##0.00";
    }

    private static void WriteMetric(
        IXLWorksheet worksheet,
        string labelAddress,
        string valueAddress,
        string label,
        XLCellValue value,
        string numberFormat)
    {
        var labelRange = worksheet.Range(labelAddress).Merge();
        var valueRange = worksheet.Range(valueAddress).Merge();
        StyleMetric(labelRange, valueRange, label);
        valueRange.FirstCell().Value = value;
        valueRange.FirstCell().Style.NumberFormat.Format = numberFormat;
    }

    private static void StyleMetric(IXLRange labelRange, IXLRange valueRange, string label)
    {
        labelRange.Value = label;
        labelRange.Style.Fill.BackgroundColor = PaleBlue;
        labelRange.Style.Font.Bold = true;
        labelRange.Style.Font.FontColor = Accent;
        labelRange.Style.Font.FontSize = 9;
        labelRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        labelRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        labelRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
        labelRange.Style.Border.TopBorderColor = Line;
        labelRange.Style.Border.LeftBorderColor = Line;
        labelRange.Style.Border.RightBorderColor = Line;
        labelRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

        valueRange.Style.Fill.BackgroundColor = White;
        valueRange.Style.Font.Bold = true;
        valueRange.Style.Font.FontColor = Navy;
        valueRange.Style.Font.FontSize = 20;
        valueRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        valueRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        valueRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
        valueRange.Style.Border.BottomBorderColor = Line;
        valueRange.Style.Border.LeftBorderColor = Line;
        valueRange.Style.Border.RightBorderColor = Line;
        valueRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        worksheetRows(valueRange).Height = 23;
    }

    private static IXLRows worksheetRows(IXLRange range) =>
        range.Worksheet.Rows(range.RangeAddress.FirstAddress.RowNumber, range.RangeAddress.LastAddress.RowNumber);

    private static int WriteBestSellers(IXLWorksheet worksheet, ReportsViewModel report)
    {
        const int sectionRow = 10;
        const int headerRow = 11;
        var sectionTitle = worksheet.Range($"A{sectionRow}:F{sectionRow}").Merge();
        sectionTitle.Value = "BEST SELLERS";
        StyleSectionTitle(sectionTitle);

        var headers = new[] { "Rank", "Product", "Units", "Revenue", "Share", "Revenue / unit" };
        for (var column = 1; column <= headers.Length; column++)
        {
            worksheet.Cell(headerRow, column).Value = headers[column - 1];
        }

        if (report.BestSellers.Count == 0)
        {
            StyleEmptyTable(worksheet, headerRow, 1, 6, "No sales were recorded in this period.");
            return headerRow + 2;
        }

        for (var index = 0; index < report.BestSellers.Count; index++)
        {
            var rowNumber = headerRow + index + 1;
            var row = report.BestSellers[index];
            worksheet.Cell(rowNumber, 1).Value = index + 1;
            worksheet.Cell(rowNumber, 2).SetValue(row.Name);
            worksheet.Cell(rowNumber, 3).Value = row.Units;
            worksheet.Cell(rowNumber, 4).Value = row.Revenue;
            worksheet.Cell(rowNumber, 5).FormulaA1 = $"=IF($A$7=0,0,D{rowNumber}/$A$7)";
            worksheet.Cell(rowNumber, 6).FormulaA1 = $"=IF(C{rowNumber}=0,0,D{rowNumber}/C{rowNumber})";
        }

        var lastDataRow = headerRow + report.BestSellers.Count;
        var table = worksheet.Range(headerRow, 1, lastDataRow, 6).CreateTable("BestSellers");
        table.Theme = XLTableTheme.TableStyleMedium2;
        table.ShowTotalsRow = true;
        table.Field("Product").TotalsRowLabel = "Total";
        table.Field("Units").TotalsRowFunction = XLTotalsRowFunction.Sum;
        table.Field("Revenue").TotalsRowFunction = XLTotalsRowFunction.Sum;
        table.Field("Share").TotalsRowFunction = XLTotalsRowFunction.Sum;

        worksheet.Range(headerRow + 1, 4, lastDataRow + 1, 4).Style.NumberFormat.Format = "$#,##0.00";
        worksheet.Range(headerRow + 1, 5, lastDataRow + 1, 5).Style.NumberFormat.Format = "0.0%";
        worksheet.Range(headerRow + 1, 6, lastDataRow, 6).Style.NumberFormat.Format = "$#,##0.00";
        worksheet.Range(headerRow, 1, lastDataRow + 1, 6).Style.Border.InsideBorderColor = Line;
        worksheet.Rows(headerRow, lastDataRow + 1).Height = 21;
        return lastDataRow + 1;
    }

    private static int WriteCategories(IXLWorksheet worksheet, ReportsViewModel report)
    {
        const int sectionRow = 10;
        const int headerRow = 11;
        var sectionTitle = worksheet.Range($"H{sectionRow}:J{sectionRow}").Merge();
        sectionTitle.Value = "CATEGORY MIX";
        StyleSectionTitle(sectionTitle);

        worksheet.Cell(headerRow, 8).Value = "Category";
        worksheet.Cell(headerRow, 9).Value = "Units";
        worksheet.Cell(headerRow, 10).Value = "Revenue";

        if (report.CategorySales.Count == 0)
        {
            StyleEmptyTable(worksheet, headerRow, 8, 10, "No category sales in this period.");
            return headerRow + 2;
        }

        for (var index = 0; index < report.CategorySales.Count; index++)
        {
            var rowNumber = headerRow + index + 1;
            var row = report.CategorySales[index];
            worksheet.Cell(rowNumber, 8).SetValue(row.Name);
            worksheet.Cell(rowNumber, 9).Value = row.Units;
            worksheet.Cell(rowNumber, 10).Value = row.Revenue;
        }

        var lastDataRow = headerRow + report.CategorySales.Count;
        var table = worksheet.Range(headerRow, 8, lastDataRow, 10).CreateTable("CategorySales");
        table.Theme = XLTableTheme.TableStyleMedium2;
        table.ShowTotalsRow = true;
        table.Field("Category").TotalsRowLabel = "Total";
        table.Field("Units").TotalsRowFunction = XLTotalsRowFunction.Sum;
        table.Field("Revenue").TotalsRowFunction = XLTotalsRowFunction.Sum;

        worksheet.Range(headerRow + 1, 10, lastDataRow + 1, 10).Style.NumberFormat.Format = "$#,##0.00";
        worksheet.Range(headerRow, 8, lastDataRow + 1, 10).Style.Border.InsideBorderColor = Line;
        worksheet.Rows(headerRow, lastDataRow + 1).Height = 21;
        return lastDataRow + 1;
    }

    private static void StyleSectionTitle(IXLRange range)
    {
        range.Style.Font.Bold = true;
        range.Style.Font.FontColor = Navy;
        range.Style.Font.FontSize = 12;
        range.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
        range.Style.Border.BottomBorderColor = Accent;
    }

    private static void StyleEmptyTable(
        IXLWorksheet worksheet,
        int headerRow,
        int firstColumn,
        int lastColumn,
        string message)
    {
        var header = worksheet.Range(headerRow, firstColumn, headerRow, lastColumn);
        header.Style.Fill.BackgroundColor = Navy;
        header.Style.Font.Bold = true;
        header.Style.Font.FontColor = White;

        var empty = worksheet.Range(headerRow + 1, firstColumn, headerRow + 2, lastColumn).Merge();
        empty.Value = message;
        empty.Style.Fill.BackgroundColor = SoftSurface;
        empty.Style.Font.FontColor = Muted;
        empty.Style.Font.Italic = true;
        empty.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        empty.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        empty.Style.Border.OutsideBorderColor = Line;
    }

    private static void WriteFooter(IXLWorksheet worksheet, int rowNumber)
    {
        var footer = worksheet.Range(rowNumber, 1, rowNumber, 10).Merge();
        footer.Value = "Aura Commerce  •  Internal sales report  •  Revenue excludes cancelled orders";
        footer.Style.Fill.BackgroundColor = SoftSurface;
        footer.Style.Font.FontColor = Muted;
        footer.Style.Font.FontSize = 9;
        footer.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        worksheet.Row(rowNumber).Height = 22;
    }
}
