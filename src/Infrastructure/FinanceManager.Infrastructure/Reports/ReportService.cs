using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Features.Reports.DTOs;
using FinanceManager.Application.Common.Models;
using FinanceManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ClosedXML.Excel;
using FinanceManager.Domain.Enums;

namespace FinanceManager.Infrastructure.Reports;

public class ReportService(ApplicationDbContext context) : IReportService
{
    public async Task<Result<ReportResultDto>> GenerateReportAsync(Guid userId, ReportRequestDto request, CancellationToken cancellationToken = default)
    {
        byte[] content;
        string contentType;
        string fileName;

        switch (request.OutputFormat.ToLower())
        {
            case "pdf":
                content = await GeneratePdfAsync(userId, request, cancellationToken);
                contentType = "application/pdf";
                fileName = $"FinanceReport_{DateTime.Now:yyyyMMdd}.pdf";
                break;
            case "excel":
            case "xlsx":
                content = await GenerateExcelAsync(userId, request, cancellationToken);
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                fileName = $"FinanceReport_{DateTime.Now:yyyyMMdd}.xlsx";
                break;
            default:
                return Result<ReportResultDto>.Failure("Unsupported output format. Use 'pdf' or 'excel'.");
        }

        return Result<ReportResultDto>.Success(new ReportResultDto(
            fileName, contentType, content, request.ReportType, DateTime.UtcNow));
    }

    public async Task<byte[]> GeneratePdfAsync(Guid userId, ReportRequestDto request, CancellationToken cancellationToken = default)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var data = await GetFinancialSummaryAsync(userId, request.FromDate, request.ToDate, cancellationToken);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(c => ComposeContent(c, data));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                    x.Span($"   •   Generated on {DateTime.Now:MMMM dd, yyyy}   •   BudgetPH — Personal Finance Manager");
                });
            });
        });

        return document.GeneratePdf();

        void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("BudgetPH").FontSize(22).Bold().FontColor("#7C3AED");
                    col.Item().Text("Personal Finance Report").FontSize(12).FontColor("#9CA3AF");
                    col.Item().Text($"{request.FromDate:MMMM dd, yyyy} – {request.ToDate:MMMM dd, yyyy}").FontSize(10).FontColor("#6B7280");
                });
                row.ConstantItem(100).AlignRight().Column(col =>
                {
                    col.Item().Text(DateTime.Now.ToString("MMM dd, yyyy")).FontSize(9).FontColor("#9CA3AF");
                });
            });
            container.PaddingBottom(10).LineHorizontal(1).LineColor("#7C3AED");
        }

        void ComposeContent(IContainer container, FinancialSummaryReportDto d)
        {
            container.PaddingTop(10).Column(col =>
            {
                // Summary Cards
                col.Item().Row(row =>
                {
                    AddSummaryCard(row.RelativeItem(), "Total Income", $"₱{d.TotalIncome:N2}", "#22c55e");
                    row.ConstantItem(8);
                    AddSummaryCard(row.RelativeItem(), "Total Expenses", $"₱{d.TotalExpenses:N2}", "#ef4444");
                    row.ConstantItem(8);
                    AddSummaryCard(row.RelativeItem(), "Net Savings", $"₱{d.NetSavings:N2}", d.NetSavings >= 0 ? "#7C3AED" : "#ef4444");
                    row.ConstantItem(8);
                    AddSummaryCard(row.RelativeItem(), "Savings Rate", $"{d.SavingsRate:F1}%", "#F59E0B");
                });

                col.Item().PaddingTop(16).Text("Expense Breakdown").FontSize(12).Bold().FontColor("#1F2937");
                col.Item().PaddingTop(6).Table(table =>
                {
                    table.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(); c.RelativeColumn(); });
                    table.Header(h =>
                    {
                        h.Cell().Background("#7C3AED").Padding(6).Text("Category").FontColor(Colors.White).Bold();
                        h.Cell().Background("#7C3AED").Padding(6).AlignRight().Text("Amount").FontColor(Colors.White).Bold();
                        h.Cell().Background("#7C3AED").Padding(6).AlignRight().Text("%").FontColor(Colors.White).Bold();
                    });
                    var isAlt = false;
                    foreach (var item in d.ExpenseBreakdown)
                    {
                        string bg = isAlt ? "#F9FAFB" : "#FFFFFF";
                        table.Cell().Background(bg).Padding(5).Text(item.Category);
                        table.Cell().Background(bg).Padding(5).AlignRight().Text($"₱{item.Amount:N2}");
                        table.Cell().Background(bg).Padding(5).AlignRight().Text($"{item.Percentage:F1}%");
                        isAlt = !isAlt;
                    }
                });

                col.Item().PaddingTop(16).Text("Monthly Trend").FontSize(12).Bold().FontColor("#1F2937");
                col.Item().PaddingTop(6).Table(table =>
                {
                    table.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
                    table.Header(h =>
                    {
                        h.Cell().Background("#7C3AED").Padding(6).Text("Month").FontColor(Colors.White).Bold();
                        h.Cell().Background("#7C3AED").Padding(6).AlignRight().Text("Income").FontColor(Colors.White).Bold();
                        h.Cell().Background("#7C3AED").Padding(6).AlignRight().Text("Expenses").FontColor(Colors.White).Bold();
                        h.Cell().Background("#7C3AED").Padding(6).AlignRight().Text("Savings").FontColor(Colors.White).Bold();
                    });
                    var isAlt = false;
                    foreach (var m in d.MonthlyTrend)
                    {
                        string bg = isAlt ? "#F9FAFB" : "#FFFFFF";
                        table.Cell().Background(bg).Padding(5).Text(m.Month);
                        table.Cell().Background(bg).Padding(5).AlignRight().Text($"₱{m.Income:N2}");
                        table.Cell().Background(bg).Padding(5).AlignRight().Text($"₱{m.Expenses:N2}");
                        var savingsColor = m.Savings >= 0 ? "#22c55e" : "#ef4444";
                        table.Cell().Background(bg).Padding(5).AlignRight().Text($"₱{m.Savings:N2}").FontColor(savingsColor);
                        isAlt = !isAlt;
                    }
                });
            });
        }

        void AddSummaryCard(IContainer c, string label, string value, string color)
        {
            c.Border(1).BorderColor("#E5E7EB").Padding(10).Column(col =>
            {
                col.Item().Text(label).FontSize(9).FontColor("#6B7280");
                col.Item().PaddingTop(4).Text(value).FontSize(14).Bold().FontColor(color);
            });
        }
    }

    public async Task<byte[]> GenerateExcelAsync(Guid userId, ReportRequestDto request, CancellationToken cancellationToken = default)
    {
        var data = await GetFinancialSummaryAsync(userId, request.FromDate, request.ToDate, cancellationToken);
        var transactions = await GetTransactionsAsync(userId, request.FromDate, request.ToDate, cancellationToken);

        using var workbook = new XLWorkbook();

        // Summary sheet
        var ws = workbook.Worksheets.Add("Summary");
        ws.Cell("A1").Value = "BudgetPH — Financial Report";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 16;
        ws.Cell("A1").Style.Font.FontColor = XLColor.FromHtml("#7C3AED");

        ws.Cell("A3").Value = $"Period: {request.FromDate:MMM dd, yyyy} – {request.ToDate:MMM dd, yyyy}";
        ws.Cell("A3").Style.Font.Italic = true;

        ws.Cell("A5").Value = "Total Income"; ws.Cell("B5").Value = data.TotalIncome;
        ws.Cell("A6").Value = "Total Expenses"; ws.Cell("B6").Value = data.TotalExpenses;
        ws.Cell("A7").Value = "Net Savings"; ws.Cell("B7").Value = data.NetSavings;
        ws.Cell("A8").Value = "Savings Rate (%)"; ws.Cell("B8").Value = data.SavingsRate;

        ws.Columns("A:B").AdjustToContents();
        ws.Range("B5:B8").Style.NumberFormat.Format = "₱#,##0.00";
        ws.Cell("A5").Style.Font.Bold = true;
        ws.Cell("A6").Style.Font.Bold = true;
        ws.Cell("A7").Style.Font.Bold = true;

        // Expense Breakdown
        var ws2 = workbook.Worksheets.Add("Expense Breakdown");
        ws2.Cell("A1").Value = "Category"; ws2.Cell("B1").Value = "Amount"; ws2.Cell("C1").Value = "% of Total";
        var headerRange = ws2.Range("A1:C1");
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#7C3AED");
        headerRange.Style.Font.FontColor = XLColor.White;

        var row = 2;
        foreach (var e in data.ExpenseBreakdown)
        {
            ws2.Cell(row, 1).Value = e.Category;
            ws2.Cell(row, 2).Value = e.Amount;
            ws2.Cell(row, 3).Value = e.Percentage / 100;
            row++;
        }
        ws2.Range(2, 2, row - 1, 2).Style.NumberFormat.Format = "₱#,##0.00";
        ws2.Range(2, 3, row - 1, 3).Style.NumberFormat.Format = "0.00%";
        ws2.Columns("A:C").AdjustToContents();

        // Transactions sheet
        var ws3 = workbook.Worksheets.Add("Transactions");
        ws3.Cell("A1").Value = "Date"; ws3.Cell("B1").Value = "Account"; ws3.Cell("C1").Value = "Category";
        ws3.Cell("D1").Value = "Description"; ws3.Cell("E1").Value = "Amount"; ws3.Cell("F1").Value = "Type";
        var tHeader = ws3.Range("A1:F1");
        tHeader.Style.Font.Bold = true;
        tHeader.Style.Fill.BackgroundColor = XLColor.FromHtml("#7C3AED");
        tHeader.Style.Font.FontColor = XLColor.White;

        var tRow = 2;
        foreach (var t in transactions)
        {
            ws3.Cell(tRow, 1).Value = t.Date;
            ws3.Cell(tRow, 2).Value = t.Account;
            ws3.Cell(tRow, 3).Value = t.Category;
            ws3.Cell(tRow, 4).Value = t.Description;
            ws3.Cell(tRow, 5).Value = t.Amount;
            ws3.Cell(tRow, 6).Value = t.Type;
            tRow++;
        }
        ws3.Range(2, 5, tRow - 1, 5).Style.NumberFormat.Format = "₱#,##0.00";
        ws3.Columns("A:F").AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    private async Task<FinancialSummaryReportDto> GetFinancialSummaryAsync(Guid userId, DateTime from, DateTime to, CancellationToken ct)
    {
        var transactions = await context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .Where(t => t.UserId == userId && t.TransactionDate >= from && t.TransactionDate <= to)
            .ToListAsync(ct);

        var totalIncome = transactions.Where(t => t.TransactionType == TransactionType.Income || t.TransactionType == TransactionType.Dividend).Sum(t => t.Amount);
        var totalExpenses = transactions.Where(t => t.TransactionType == TransactionType.Expense || t.TransactionType == TransactionType.Fee).Sum(t => t.Amount);
        var netSavings = totalIncome - totalExpenses;
        var savingsRate = totalIncome > 0 ? (netSavings / totalIncome) * 100 : 0;

        var expenseBreakdown = transactions
            .Where(t => t.TransactionType == TransactionType.Expense && t.Category != null)
            .GroupBy(t => t.Category!.Name)
            .Select(g => new ExpenseBreakdownDto(g.Key, g.Sum(t => t.Amount), totalExpenses > 0 ? g.Sum(t => t.Amount) / totalExpenses * 100 : 0))
            .OrderByDescending(e => e.Amount).ToList();

        var monthlyTrend = transactions
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
            .Select(g => new MonthlyTrendDto(
                $"{new DateTime(g.Key.Year, g.Key.Month, 1):MMM yyyy}",
                g.Where(t => t.TransactionType == TransactionType.Income).Sum(t => t.Amount),
                g.Where(t => t.TransactionType == TransactionType.Expense).Sum(t => t.Amount),
                g.Where(t => t.TransactionType == TransactionType.Income).Sum(t => t.Amount) -
                g.Where(t => t.TransactionType == TransactionType.Expense).Sum(t => t.Amount)))
            .OrderBy(m => m.Month).ToList();

        return new FinancialSummaryReportDto(
            $"{from:MMM yyyy} – {to:MMM yyyy}", from, to,
            totalIncome, totalExpenses, netSavings, savingsRate,
            [], expenseBreakdown, monthlyTrend, 0, 0, 0);
    }

    private async Task<List<(DateTime Date, string Account, string Category, string Description, decimal Amount, string Type)>> GetTransactionsAsync(
        Guid userId, DateTime from, DateTime to, CancellationToken ct)
    {
        return await context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .Where(t => t.UserId == userId && t.TransactionDate >= from && t.TransactionDate <= to)
            .OrderByDescending(t => t.TransactionDate)
            .Select(t => ValueTuple.Create(
                t.TransactionDate,
                t.Account.Name,
                t.Category != null ? t.Category.Name : "Uncategorized",
                t.Description,
                t.Amount,
                t.TransactionType.ToString()))
            .ToListAsync(ct);
    }
}
