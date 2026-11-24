using AQUA.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using AQUA.Controllers;
using AQUA.Models;
using AQUA.ViewModels;

namespace YourProjectName.Controllers
{
    public class FinancialController : Controller
    {
        private readonly ApplicationDbContext _db;

        public FinancialController(ApplicationDbContext db) => _db = db;

        // Dashboard function with date filtering
        [HttpGet]
        public async Task<IActionResult> Dashboard([FromQuery] FinancialReportFilterVm filter)
        {
            // Default to current month if no date is provided
            var from = filter.FromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var to = filter.ToDate ?? DateTime.Today;

            // Base invoices query with date range filter
            var invoicesQ = _db.Invoices.AsNoTracking()
                .Where(i => i.IssueDate.Date >= from.Date && i.IssueDate.Date <= to.Date);

            // Apply additional filters if provided
            if (!string.IsNullOrWhiteSpace(filter.Username))
                invoicesQ = invoicesQ.Where(i => i.FullName.Contains(filter.Username));

            if (!string.IsNullOrWhiteSpace(filter.InvoiceStatus) && filter.InvoiceStatus != "All")
                invoicesQ = invoicesQ.Where(i => i.Status == filter.InvoiceStatus);

            // Payments query with date range filter
            var paymentsQ = _db.Payments.AsNoTracking()
                .Where(p => p.PaymentDate.Date >= from.Date && p.PaymentDate.Date <= to.Date);

            if (!string.IsNullOrWhiteSpace(filter.PaymentMethod) && filter.PaymentMethod != "All")
                paymentsQ = paymentsQ.Where(p => p.Method == filter.PaymentMethod);

            // Prepare the data for the dashboard table rows
            var rows = await invoicesQ
                .Select(i => new InvoiceReportRowVm
                {
                    InvoiceId = i.InvoiceId,
                    InvoiceNumber = i.InvoiceNumber,
                    Username = i.FullName,
                    IssueDate = i.IssueDate,
                    DueDate = i.DueDate,
                    TotalAmount = i.TotalAmount,
                    PaidAmount = _db.Payments
                        .Where(p => p.InvoiceId == i.InvoiceId)
                        .Sum(p => (decimal?)p.Amount) ?? 0m,
                    Status = i.Status,
                    IsOverdue = i.DueDate < DateTime.Today && i.Status != "Paid"
                })
                .OrderByDescending(r => r.IssueDate)
                .ToListAsync();

            // Calculate the summary for the dashboard
            var totalInvoices = rows.Sum(r => r.TotalAmount);
            var totalPayments = rows.Sum(r => r.PaidAmount);
            var outstanding = rows.Sum(r => r.Balance);
            var overdueCount = rows.Count(r => r.IsOverdue);

            // Prepare data for the chart (monthly totals)
            var monthStart = new DateTime(from.Year, from.Month, 1);
            var monthEnd = new DateTime(to.Year, to.Month, 1).AddMonths(1).AddDays(-1);

            var labels = new List<string>();
            var invoiceSeries = new List<decimal>();
            var paymentSeries = new List<decimal>();

            // Get the monthly totals
            for (var dt = monthStart; dt <= monthEnd; dt = dt.AddMonths(1))
            {
                var mFrom = dt;
                var mTo = dt.AddMonths(1).AddDays(-1);

                labels.Add(dt.ToString("MMM yyyy"));

                var invMonth = await invoicesQ
                    .Where(i => i.IssueDate.Date >= mFrom.Date && i.IssueDate.Date <= mTo.Date)
                    .SumAsync(i => (decimal?)i.TotalAmount) ?? 0m;

                var payMonth = await paymentsQ
                    .Where(p => p.PaymentDate.Date >= mFrom.Date && p.PaymentDate.Date <= mTo.Date)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0m;

                invoiceSeries.Add(invMonth);
                paymentSeries.Add(payMonth);
            }

            // Prepare the ViewModel for the dashboard
            var vm = new FinancialDashboardVm
            {
                Filter = new FinancialReportFilterVm
                {
                    FromDate = from,
                    ToDate = to,
                    InvoiceStatus = filter.InvoiceStatus ?? "All",
                    PaymentMethod = filter.PaymentMethod ?? "All",
                    Username = filter.Username
                },
                TotalInvoices = totalInvoices,
                TotalPayments = totalPayments,
                Outstanding = outstanding,
                OverdueCount = overdueCount,
                Rows = rows,
                MonthLabels = labels,
                MonthlyInvoiceTotals = invoiceSeries,
                MonthlyPaymentTotals = paymentSeries
            };

            return View(vm);
        }

        public async Task<IActionResult> ExportCsv([FromQuery] FinancialReportFilterVm filter)
        {
            var from = filter.FromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var to = filter.ToDate ?? DateTime.Today;

            // Base invoices query with date range filter
            var invoicesQ = _db.Invoices.AsNoTracking()
                .Where(i => i.IssueDate.Date >= from.Date && i.IssueDate.Date <= to.Date);

            // Apply additional filters if provided
            if (!string.IsNullOrWhiteSpace(filter.Username))
                invoicesQ = invoicesQ.Where(i => i.FullName.Contains(filter.Username));

            if (!string.IsNullOrWhiteSpace(filter.InvoiceStatus) && filter.InvoiceStatus != "All")
                invoicesQ = invoicesQ.Where(i => i.Status == filter.InvoiceStatus);

            // Fetch payments as well (for generating paid amounts)
            var paymentsQ = _db.Payments.AsNoTracking()
                .Where(p => p.PaymentDate.Date >= from.Date && p.PaymentDate.Date <= to.Date);

            if (!string.IsNullOrWhiteSpace(filter.PaymentMethod) && filter.PaymentMethod != "All")
                paymentsQ = paymentsQ.Where(p => p.Method == filter.PaymentMethod);

            // Collect the data for each invoice
            var rows = await invoicesQ
                .Select(i => new InvoiceReportRowVm
                {
                    InvoiceId = i.InvoiceId,
                    InvoiceNumber = i.InvoiceNumber,
                    Username = i.FullName,
                    IssueDate = i.IssueDate,
                    DueDate = i.DueDate,
                    TotalAmount = i.TotalAmount,
                    PaidAmount = _db.Payments
                        .Where(p => p.InvoiceId == i.InvoiceId)
                        .Sum(p => (decimal?)p.Amount) ?? 0m,
                    Status = i.Status,
                    IsOverdue = i.DueDate < DateTime.Today && i.Status != "Paid"
                })
                .ToListAsync();

            // Prepare the CSV headers and rows
            var csvLines = new List<string>
    {
        "Invoice Number,Username,Issue Date,Due Date,Total Amount,Paid Amount,Balance,Status,Overdue"
    };

            // Add each row's data to the CSV
            foreach (var row in rows)
            {
                csvLines.Add($"{row.InvoiceNumber},{row.Username},{row.IssueDate:yyyy-MM-dd},{row.DueDate:yyyy-MM-dd},{row.TotalAmount},{row.PaidAmount},{row.Balance},{row.Status},{(row.IsOverdue ? "YES" : "NO")}");
            }

            // Create the CSV content
            var csvContent = string.Join("\n", csvLines);

            // Return the CSV file for download
            return File(System.Text.Encoding.UTF8.GetBytes(csvContent), "text/csv", "Financial_Report.csv");
        }
    }
}