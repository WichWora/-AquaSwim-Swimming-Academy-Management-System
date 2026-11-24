using AQUA.Models;
using AQUA.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AQUA.Controllers
{
    [Authorize]
    public class InvoiceController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly Helper _hp;

        public InvoiceController(ApplicationDbContext db, Helper hp)
        {
            _db = db;
            _hp = hp;
        }

        public async Task<IActionResult> Index()
        {
            // If student, only show their own invoices
            if (User.IsInRole("Student"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var invoices = await _db.Invoices
                    .Where(i => i.UserId == userId)
                    .Include(i => i.Payments)
                    .ToListAsync();
                return View(invoices);
            }
            else
            {
                // Managers/Admins see all invoices
                var invoices = await _db.Invoices
                    .Include(i => i.Payments)
                    .Include(i => i.User)
                    .ToListAsync();
                return View(invoices);
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            var invoice = await _db.Invoices
                .Include(i => i.User)
                .Include(i => i.Payments)
                .Include(i => i.Schedule)
                    .ThenInclude(s => s.Class)
                .Include(i => i.Schedule)
                    .ThenInclude(s => s.Coach)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null)
            {
                TempData["Error"] = "Invoice not found.";
                return RedirectToAction("Index");
            }

            // Authorization check: Students can only see their own invoices
            if (User.IsInRole("Student"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (invoice.UserId != userId)
                {
                    return Forbid();
                }
            }

            return View(invoice);
        }

        [HttpGet]
        public async Task<IActionResult> Insert(int? scheduleId, decimal? promoPrice)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var fullName = User.FindFirstValue("FullName");

            // If scheduleId is provided, get class info for the price
            Schedule? schedule = null;
            if (scheduleId.HasValue)
            {
                schedule = await _db.Schedules
                    .Include(s => s.Class)
                    .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId.Value);
            }

            string nextInvoiceId = await GenerateNextInvoiceId();
            string invoiceNumber = $"{DateTime.Today:yyyyMMdd}-{nextInvoiceId}";

            // If no scheduleId provided, load schedules for selection (e.g., from promotion)
            if (!scheduleId.HasValue)
            {
                ViewBag.Schedules = await _db.Schedules
                    .Include(s => s.Class)
                    .Include(s => s.Coach)
                    .Where(s => s.CurrentEnrollment < s.Class.MaxCapacity)
                    .Select(s => new {
                        Id = s.ScheduleId,
                        Display = $"{s.Class.ClassName} - {s.DayOfWeek} {s.StartTime:hh\\:mm} ({s.Coach.FullName})"
                    })
                    .ToListAsync();
            }

            var vm = new InvoiceVM
            {
                InvoiceId = nextInvoiceId,
                UserId = userId,
                FullName = fullName,
                InvoiceNumber = invoiceNumber,
                TotalAmount = promoPrice ?? schedule?.Class?.Fee ?? 399.00m,
                IssueDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(14),
                ScheduleId = scheduleId,
                Status = "Unpaid"
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Insert(InvoiceVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _db.Users.FindAsync(vm.UserId);
            if (user == null)
            {
                ModelState.AddModelError("UserId", "User not found.");
                return View(vm);
            }

            // Final check for duplicate ID
            if (await _db.Invoices.AnyAsync(i => i.InvoiceId == vm.InvoiceId))
            {
                vm.InvoiceId = await GenerateNextInvoiceId();
                vm.InvoiceNumber = $"{DateTime.Today:yyyyMMdd}-{vm.InvoiceId}";
            }

            var invoice = new Invoice
            {
                InvoiceId = vm.InvoiceId,
                UserId = vm.UserId,
                FullName = user.FullName,
                InvoiceNumber = vm.InvoiceNumber ?? Guid.NewGuid().ToString().ToUpper().Substring(0, 8),
                TotalAmount = vm.TotalAmount,
                IssueDate = vm.IssueDate,
                DueDate = vm.DueDate,
                ScheduleId = vm.ScheduleId,
                Status = vm.Status ?? "Unpaid"
            };

            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Invoice {invoice.InvoiceId} created successfully.";
            return RedirectToAction("Index");
        }

        private async Task<string> GenerateNextInvoiceId()
        {
            var existingIds = await _db.Invoices.Select(i => i.InvoiceId).ToListAsync();
            int nextNum = 1;

            if (existingIds.Any())
            {
                nextNum = existingIds.Select(id =>
                {
                    // Handle transition from legacy "INV####" to numeric "#####"
                    string numericPart = id.StartsWith("INV") ? id.Substring(3) : id;
                    return int.TryParse(numericPart, out int n) ? n : 0;
                }).Max() + 1;
            }

            return nextNum.ToString("D5");
        }
    }
}
