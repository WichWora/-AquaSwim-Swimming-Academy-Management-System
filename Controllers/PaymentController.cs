using AQUA.Models;
using AQUA.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AQUA.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PaymentController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Pay(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Invoice ID is required.";
                return RedirectToAction("Index", "Invoice");
            }

            var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == id);
            if (invoice == null)
            {
                TempData["Error"] = "Invoice not found.";
                return RedirectToAction("Index", "Invoice");
            }

            if (invoice.Status == "Paid")
            {
                TempData["Success"] = "This invoice is already paid.";
                return RedirectToAction("Details", "Invoice", new { id = id });
            }

            // Authorization check
            if (User.IsInRole("Student"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (invoice.UserId != userId) return Forbid();
            }

            var vm = new PaymentVM
            {
                InvoiceId = invoice.InvoiceId,
                Amount = invoice.TotalAmount
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(PaymentVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == vm.InvoiceId);
            if (invoice == null)
            {
                TempData["Error"] = "Invoice not found.";
                return RedirectToAction("Index", "Invoice");
            }

            // Update Invoice Status
            invoice.Status = "Paid";

            // Create Payment Record
            var maxIdStr = await _db.Payments
                .Where(p => p.PaymentId.StartsWith("PY"))
                .OrderByDescending(p => p.PaymentId)
                .Select(p => p.PaymentId)
                .FirstOrDefaultAsync();

            int nextNum = 1;
            if (maxIdStr != null && maxIdStr.Length > 2)
            {
                if (int.TryParse(maxIdStr.Substring(2), out int currentMax))
                    nextNum = currentMax + 1;
            }
            string newPaymentId = $"PY{nextNum:D4}";

            var payment = new Payment
            {
                PaymentId = newPaymentId,
                InvoiceId = invoice.InvoiceId,
                Amount = invoice.TotalAmount,
                PaymentDate = DateTime.Now,
                Method = "Card",
                Status = "Success"
            };

            _db.Payments.Add(payment);

            // Link Enrollment if this invoice is for a schedule
            if (invoice.ScheduleId.HasValue)
            {
                // Create Enrollment Record
                var enrollment = new Enrollment
                {
                    ScheduleId = invoice.ScheduleId.Value,
                    StudentId = invoice.UserId,
                    EnrollmentDate = DateTime.Now,
                    Status = "Enrolled"
                };
                _db.Enrollments.Add(enrollment);
                await _db.SaveChangesAsync(); // Save to get enrollment.EnrollmentId

                // Link Enrollment to Invoice
                invoice.EnrollmentId = enrollment.EnrollmentId;

                // Update Schedule CurrentEnrollment
                var schedule = await _db.Schedules.FindAsync(invoice.ScheduleId.Value);
                if (schedule != null)
                {
                    schedule.CurrentEnrollment++;
                    _db.Entry(schedule).State = EntityState.Modified;
                }
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = $"Payment of {payment.Amount:C} processed successfully for Invoice {invoice.InvoiceId}. Student enrolled successfully.";
            return RedirectToAction("Details", "Invoice", new { id = invoice.InvoiceId });
        }
    }
}
