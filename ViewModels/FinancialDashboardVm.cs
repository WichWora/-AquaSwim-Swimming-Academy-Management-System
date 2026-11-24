namespace AQUA.ViewModels
{
    public class FinancialDashboardVm
    {
        public FinancialReportFilterVm Filter { get; set; } // This needs to be here

        // Other properties like totals, rows, etc.
        public decimal TotalInvoices { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal Outstanding { get; set; }
        public int OverdueCount { get; set; }
        public List<InvoiceReportRowVm> Rows { get; set; } = new();
        public List<string> MonthLabels { get; set; } = new();
        public List<decimal> MonthlyInvoiceTotals { get; set; } = new();
        public List<decimal> MonthlyPaymentTotals { get; set; } = new();
    }

    public class FinancialReportFilterVm
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? InvoiceStatus { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Username { get; set; }
    }
}