using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AQUA.Models;

namespace AQUA.Controllers
{
    [Authorize(Roles = "Manager,ServerAdmin,Admin")]
    public class SchedulesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SchedulesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Schedules
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var schedules = await _context.Schedules
                .Include(s => s.Class)
                .Include(s => s.Coach)
                .Where(s => s.StartDate >= DateTime.Today)
                .OrderBy(s => s.StartDate)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            return View(schedules);
        }

        // GET: Schedules/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schedule = await _context.Schedules
                .Include(s => s.Class)
                .Include(s => s.Coach)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(m => m.ScheduleId == id);

            if (schedule == null)
            {
                return NotFound();
            }

            // Calculate available seats
            ViewBag.AvailableSeats = schedule.Class?.MaxCapacity - schedule.CurrentEnrollment ?? 0;
            ViewBag.TotalEnrollments = schedule.Enrollments?.Count ?? 0;

            return View(schedule);
        }

        // GET: Schedules/Create
        public async Task<IActionResult> Create()
        {
            // Call the corrected PrepareViewData
            await PrepareViewData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    [Bind("ClassId,CoachId,StartDate,EndDate,DayOfWeek,StartTime,EndTime,PoolLocation")] Schedule schedule,
    string scheduleType)  // Add scheduleType parameter from form
        {
            Console.WriteLine($"=== CREATE SCHEDULE STARTED ===");
            Console.WriteLine($"ClassId: {schedule.ClassId}, CoachId: {schedule.CoachId}");
            Console.WriteLine($"Schedule Type from form: {scheduleType}");
            Console.WriteLine($"StartDate: {schedule.StartDate}, EndDate: {schedule.EndDate}");
            Console.WriteLine($"DayOfWeek: {schedule.DayOfWeek}");

            // FIX: Remove navigation property validation
            ModelState.Remove("Class");
            ModelState.Remove("Coach");
            ModelState.Remove("Enrollments");

            // IMPORTANT: Handle single vs recurring logic
            if (scheduleType == "single")
            {
                // For single class, set EndDate = StartDate and DayOfWeek based on StartDate
                schedule.EndDate = schedule.StartDate;

                // Set DayOfWeek based on StartDate for single classes
                schedule.DayOfWeek = schedule.StartDate.DayOfWeek.ToString();

                Console.WriteLine($"Single class - Set EndDate to StartDate: {schedule.EndDate}");
                Console.WriteLine($"Single class - Set DayOfWeek to: {schedule.DayOfWeek}");
            }
            else if (scheduleType == "recurring")
            {
                // For recurring class, validate required fields
                if (string.IsNullOrEmpty(schedule.DayOfWeek))
                {
                    ModelState.AddModelError("DayOfWeek", "Day of Week is required for recurring classes.");
                }

                // Check if EndDate is provided and valid
                if (schedule.EndDate == default(DateTime) || schedule.EndDate == DateTime.MinValue)
                {
                    ModelState.AddModelError("EndDate", "End Date is required for recurring classes.");
                }
                else if (schedule.EndDate <= schedule.StartDate)
                {
                    ModelState.AddModelError("EndDate", "End Date must be after Start Date.");
                }
            }

            // Additional validation for all schedules
            if (schedule.StartDate < DateTime.Today)
            {
                ModelState.AddModelError("StartDate", "Start date cannot be in the past.");
            }

            if (schedule.StartTime >= schedule.EndTime)
            {
                ModelState.AddModelError("EndTime", "End time must be after start time.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Set default values
                    schedule.CurrentEnrollment = 0;
                    // REMOVED: schedule.IsActive = true; // Remove this line if IsActive doesn't exist

                    // Check for time conflicts
                    var conflict = await CheckScheduleConflict(schedule);
                    if (conflict != null)
                    {
                        ModelState.AddModelError(string.Empty,
                            $"Coach has a scheduling conflict with '{conflict.Class?.ClassName}' at {conflict.StartTime:h:mm tt} on {conflict.DayOfWeek}");
                        await PrepareViewData();
                        return View(schedule);
                    }

                    _context.Add(schedule);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("=== SCHEDULE CREATED SUCCESSFULLY ===");

                    TempData["SuccessMessage"] = "Schedule created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbEx)
                {
                    Console.WriteLine($"Database Error: {dbEx.Message}");
                    Console.WriteLine($"Inner Exception: {dbEx.InnerException?.Message}");
                    ModelState.AddModelError(string.Empty, $"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General Error: {ex.Message}");
                    ModelState.AddModelError(string.Empty, $"Error creating schedule: {ex.Message}");
                }
            }
            else
            {
                // Log validation errors
                Console.WriteLine("=== MODEL STATE ERRORS ===");
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    if (errors.Count > 0)
                    {
                        foreach (var error in errors)
                        {
                            Console.WriteLine($"{key}: {error.ErrorMessage}");
                        }
                    }
                }
            }

            await PrepareViewData();
            return View(schedule);
        }

        // GET: Schedules/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            await PrepareViewData();
            return View(schedule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ScheduleId,ClassId,CoachId,StartDate,EndDate,DayOfWeek,StartTime,EndTime,PoolLocation,CurrentEnrollment")] Schedule schedule)
        {
            if (id != schedule.ScheduleId)
            {
                return NotFound();
            }

            // Remove navigation property validation
            ModelState.Remove("Class");
            ModelState.Remove("Coach");
            ModelState.Remove("Enrollments");

            if (ModelState.IsValid)
            {
                try
                {
                    // Check for time conflicts (excluding current schedule)
                    var conflict = await CheckScheduleConflict(schedule, id);
                    if (conflict != null)
                    {
                        ModelState.AddModelError(string.Empty,
                            $"Coach has a scheduling conflict with another class at {conflict.StartTime:h:mm tt} on {conflict.DayOfWeek}");
                        await PrepareViewData();
                        return View(schedule);
                    }

                    _context.Update(schedule);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Schedule updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScheduleExists(schedule.ScheduleId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            await PrepareViewData();
            return View(schedule);
        }

        // GET: Schedules/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schedule = await _context.Schedules
                .Include(s => s.Class)
                .Include(s => s.Coach)
                .FirstOrDefaultAsync(m => m.ScheduleId == id);

            if (schedule == null)
            {
                return NotFound();
            }

            // Check if schedule has enrollments
            var hasEnrollments = await _context.Enrollments
                .AnyAsync(e => e.ScheduleId == id && e.Status != "Cancelled");

            ViewBag.HasEnrollments = hasEnrollments;
            ViewBag.EnrollmentCount = await _context.Enrollments
                .CountAsync(e => e.ScheduleId == id && e.Status != "Cancelled");

            return View(schedule);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schedule = await _context.Schedules
                .Include(s => s.Enrollments)
                .FirstOrDefaultAsync(s => s.ScheduleId == id);

            if (schedule != null)
            {
                // Soft delete by moving dates to past
                schedule.StartDate = schedule.StartDate.AddYears(-10);
                schedule.EndDate = schedule.EndDate.AddYears(-10);

                // Cancel all enrollments first
                foreach (var enrollment in schedule.Enrollments)
                {
                    enrollment.Status = "Cancelled";
                }

                _context.Update(schedule);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Schedule deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Schedules/Calendar
        [AllowAnonymous]
        public async Task<IActionResult> Calendar(int? year, int? month)
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(
                year ?? today.Year,
                month ?? today.Month,
                1
            );

            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            // Get all schedules that could occur in this month
            var allSchedules = await _context.Schedules
                .Include(s => s.Class)
                .Include(s => s.Coach)
                .Where(s =>
                    // One-time classes that start this month
                    (s.StartDate >= monthStart && s.StartDate <= monthEnd) ||
                    // Recurring classes that could occur this month
                    (s.DayOfWeek != null &&
                     s.StartDate <= monthEnd &&
                     (s.EndDate == null || s.EndDate >= monthStart))
                )
                .ToListAsync();

            // Filter to get actual schedules for this month
            var schedulesThisMonth = new List<Schedule>();
            foreach (var schedule in allSchedules)
            {
                if (schedule.DayOfWeek != null)
                {
                    // Recurring schedule - check which days fall in this month
                    var currentDate = schedule.StartDate;
                    while (currentDate <= monthEnd &&
                           (schedule.EndDate == null || currentDate <= schedule.EndDate))
                    {
                        if (currentDate >= monthStart &&
                            currentDate.DayOfWeek.ToString() == schedule.DayOfWeek)
                        {
                            // Create a copy for this specific date
                            var scheduleCopy = new Schedule
                            {
                                ScheduleId = schedule.ScheduleId,
                                ClassId = schedule.ClassId,
                                CoachId = schedule.CoachId,
                                StartDate = currentDate,
                                EndDate = schedule.EndDate,
                                DayOfWeek = schedule.DayOfWeek,
                                StartTime = schedule.StartTime,
                                EndTime = schedule.EndTime,
                                PoolLocation = schedule.PoolLocation,
                                CurrentEnrollment = schedule.CurrentEnrollment,
                                Class = schedule.Class,
                                Coach = schedule.Coach
                            };
                            schedulesThisMonth.Add(scheduleCopy);
                        }
                        currentDate = currentDate.AddDays(7);
                    }
                }
                else
                {
                    // One-time schedule
                    if (schedule.StartDate >= monthStart && schedule.StartDate <= monthEnd)
                    {
                        schedulesThisMonth.Add(schedule);
                    }
                }
            }

            ViewBag.MonthStart = monthStart;
            ViewBag.MonthEnd = monthEnd;

            return View(schedulesThisMonth);
        }

        // Helper method to prepare ViewData - FIXED VERSION
        private async Task PrepareViewData()
        {
            try
            {
                // Get active classes
                var classes = await _context.Classes
                    .Where(c => c.IsActive)
                    .ToListAsync();

                // Get active coaches
                var coaches = await _context.Users
                    .OfType<Coach>()
                    .Where(c => c.IsActive)
                    .ToListAsync();

                // Create SelectList for classes
                var classList = classes
                    .Select(c => new SelectListItem
                    {
                        Value = c.ClassId.ToString(),
                        Text = $"{c.ClassName} ({c.Level}) - RM{c.Fee:N2}"
                    })
                    .ToList();

                // Create SelectList for coaches
                var coachList = coaches
                    .Select(c => new SelectListItem
                    {
                        Value = c.UserId,
                        Text = c.FullName
                    })
                    .ToList();

                // Add "Select..." option
                classList.Insert(0, new SelectListItem { Value = "", Text = "-- Select Class --" });
                coachList.Insert(0, new SelectListItem { Value = "", Text = "-- Select Coach --" });

                ViewData["ClassId"] = new SelectList(classList, "Value", "Text");
                ViewData["CoachId"] = new SelectList(coachList, "Value", "Text");

                // Days of week
                var days = new[]
                {
                    new SelectListItem("Monday", "Monday"),
                    new SelectListItem("Tuesday", "Tuesday"),
                    new SelectListItem("Wednesday", "Wednesday"),
                    new SelectListItem("Thursday", "Thursday"),
                    new SelectListItem("Friday", "Friday"),
                    new SelectListItem("Saturday", "Saturday"),
                    new SelectListItem("Sunday", "Sunday")
                };

                ViewData["DaysOfWeek"] = new SelectList(days, "Value", "Text");

                // Additional info for debugging
                ViewBag.TotalClasses = classes.Count;
                ViewBag.TotalCoaches = coaches.Count;
                ViewBag.DebugMessage = $"Found {classes.Count} classes and {coaches.Count} coaches";
            }
            catch (Exception ex)
            {
                // Log error and set empty lists
                Console.WriteLine($"Error in PrepareViewData: {ex.Message}");
                ViewData["ClassId"] = new SelectList(new List<SelectListItem>());
                ViewData["CoachId"] = new SelectList(new List<SelectListItem>());
                ViewData["DaysOfWeek"] = new SelectList(new List<SelectListItem>());
                ViewBag.DebugMessage = $"Error loading data: {ex.Message}";
            }
        }

        private async Task<Schedule> CheckScheduleConflict(Schedule newSchedule, int? excludeScheduleId = null)
        {
            var query = _context.Schedules
                .Where(s => s.CoachId == newSchedule.CoachId)
                .Where(s => s.DayOfWeek == newSchedule.DayOfWeek)
                .Where(s => s.StartTime < newSchedule.EndTime && s.EndTime > newSchedule.StartTime)
                .Where(s => !(s.EndDate < newSchedule.StartDate || s.StartDate > newSchedule.EndDate));

            if (excludeScheduleId.HasValue)
            {
                query = query.Where(s => s.ScheduleId != excludeScheduleId.Value);
            }

            return await query.FirstOrDefaultAsync();
        }

        private bool ScheduleExists(int id)
        {
            return _context.Schedules.Any(e => e.ScheduleId == id);
        }
    }
}