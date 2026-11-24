using System;
using System.ComponentModel.DataAnnotations;

namespace AQUA.ViewModels
{
    public class InvoiceVM
    {
        [Display(Name = "Invoice ID")]
        public string InvoiceId { get; set; }

        [Required]
        [Display(Name = "User ID")]
        public string UserId { get; set; }

        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Display(Name = "Invoice Number")]
        public string InvoiceNumber { get; set; }

        [Required]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime IssueDate { get; set; }

        [Required]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        public int? ScheduleId { get; set; }

        public string Status { get; set; }
    }
}
