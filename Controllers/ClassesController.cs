using AQUA.Models;
using AQUA;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AQUA.Controllers
{
    [Authorize(Roles = "Manager,ServerAdmin,Admin")]
    public class ClassesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClassesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Classes
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Classes.Where(c => c.IsActive).ToListAsync());
        }

        // GET: Classes/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classObj = await _context.Classes
                .Include(c => c.Schedules)
                    .ThenInclude(s => s.Coach)
                .Include(c => c.Schedules)
                    .ThenInclude(s => s.Enrollments) // Include enrollments to calculate availability
                .FirstOrDefaultAsync(m => m.ClassId == id);

            if (classObj == null)
            {
                return NotFound();
            }

            // Calculate current enrollment for each schedule
            foreach (var schedule in classObj.Schedules)
            {
                schedule.CurrentEnrollment = schedule.Enrollments?.Count ?? 0;
            }

            return View(classObj);
        }

        // GET: Classes/Create
        public IActionResult Create()
        {
            // Set default values for new class
            var newClass = new Class
            {
                IsActive = true,  // Default to active
                MaxCapacity = 15, // Default capacity
                Fee = 200.00m,    // Default fee
                Level = "Beginner" // Add default level
            };

            return View(newClass);
        }

        // POST: Classes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClassName,Level,MaxCapacity,Fee,Description,IsActive")] Class classObj)
        {
            Console.WriteLine($"Creating class: {classObj.ClassName}, IsActive: {classObj.IsActive}");

            if (ModelState.IsValid)
            {
                try
                {
                    // ⭐ CRITICAL FIX: Initialize the Schedules collection
                    classObj.Schedules = new List<Schedule>();

                    _context.Add(classObj);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Class '{classObj.ClassName}' created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbEx)
                {
                    ModelState.AddModelError(string.Empty, $"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}");
                    Console.WriteLine($"Database error: {dbEx.Message}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Error creating class: {ex.Message}");
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            else
            {
                // Log validation errors
                Console.WriteLine("ModelState errors:");
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

            return View(classObj);
        }

        // GET: Classes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classObj = await _context.Classes.FindAsync(id);
            if (classObj == null)
            {
                return NotFound();
            }
            return View(classObj);
        }

        // POST: Classes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ClassId,ClassName,Level,MaxCapacity,Fee,Description,IsActive")] Class classObj)
        {
            if (id != classObj.ClassId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if the class exists
                    var existingClass = await _context.Classes.FindAsync(id);
                    if (existingClass == null)
                    {
                        return NotFound();
                    }

                    // Update the existing class
                    existingClass.ClassName = classObj.ClassName;
                    existingClass.Level = classObj.Level;
                    existingClass.MaxCapacity = classObj.MaxCapacity;
                    existingClass.Fee = classObj.Fee;
                    existingClass.Description = classObj.Description;
                    existingClass.IsActive = classObj.IsActive;

                    // Mark as modified
                    _context.Entry(existingClass).State = EntityState.Modified;

                    // Save changes
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Class '{classObj.ClassName}' updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException dbEx)
                {
                    if (!ClassExists(classObj.ClassId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        TempData["ErrorMessage"] = $"Concurrency error: {dbEx.Message}";
                        return View(classObj);
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error updating class: {ex.Message}";
                    return View(classObj);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(classObj);
        }

        private bool ClassExists(int id)
        {
            return _context.Classes.Any(e => e.ClassId == id);
        }
        // GET: Classes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classObj = await _context.Classes
                .Include(c => c.Schedules) // ⭐ IMPORTANT: Include schedules to show warning
                .FirstOrDefaultAsync(m => m.ClassId == id);

            if (classObj == null)
            {
                return NotFound();
            }

            // Pass schedule count to view for warning
            ViewBag.ScheduleCount = classObj.Schedules?.Count ?? 0;
            ViewBag.HasSchedules = ViewBag.ScheduleCount > 0;

            return View(classObj);
        }

        // POST: Classes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var classObj = await _context.Classes
                .Include(c => c.Schedules) // ⭐ Include related schedules
                .FirstOrDefaultAsync(c => c.ClassId == id);

            if (classObj == null)
            {
                TempData["ErrorMessage"] = "Class not found.";
                return RedirectToAction(nameof(Index));
            }

            string className = classObj.ClassName; // Store name for message

            try
            {
                // ⭐ Remove all related schedules first (cascade delete)
                if (classObj.Schedules != null && classObj.Schedules.Any())
                {
                    _context.Schedules.RemoveRange(classObj.Schedules);
                }

                // Now remove the class
                _context.Classes.Remove(classObj);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Class '{className}' has been permanently deleted.";
            }
            catch (DbUpdateException dbEx)
            {
                // If there are other dependencies (like enrollments), handle them
                TempData["ErrorMessage"] =
                    $"Cannot delete class. It may have active enrollments. Error: {dbEx.InnerException?.Message}";

                // Return to delete page to show error
                return RedirectToAction(nameof(Delete), new { id = id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting class: {ex.Message}";
                return RedirectToAction(nameof(Delete), new { id = id });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}