namespace AQUA.ViewModels
{
    public class InvoiceReportRowVm
    {
        public string InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string Username { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Balance => TotalAmount - PaidAmount;  // This calculates the balance
        public string Status { get; set; }
        public bool IsOverdue { get; set; }
    }
}
