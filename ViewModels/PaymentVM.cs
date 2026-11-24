using System;
using System.ComponentModel.DataAnnotations;

namespace AQUA.ViewModels
{
    public class PaymentVM
    {
        [Required]
        public string InvoiceId { get; set; }

        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Cardholder Name")]
        public string CardName { get; set; }

        [Required]
        [CreditCard]
        [Display(Name = "Card Number")]
        public string CardNumber { get; set; }

        [Required]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/([0-9]{2})$", ErrorMessage = "Expiration date must be in MM/YY format")]
        [Display(Name = "Expiration (MM/YY)")]
        public string ExpiryDate { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "CVV must be 3 or 4 digits")]
        public string CVV { get; set; }
    }
}
