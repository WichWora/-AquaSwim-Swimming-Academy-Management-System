using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AQUA.Models;
using System.Security.Claims;
using System.Text;

namespace AQUA.Controllers
{
    [Authorize(Roles = "Coach")]
    public class SimpleCoachController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string _attendanceFolder = "Data/Attendance";

        public SimpleCoachController(ApplicationDbContext context)
        {
            _context = context;

            // Create attendance folder if it doesn't exist
            if (!Directory.Exists(_attendanceFolder))
            {
                Directory.CreateDirectory(_attendanceFolder);
            }
        }

        // 1. HOME PAGE - Fixed to show actual coach name
        public async Task<IActionResult> Home()
        {
            var coachId = await GetCurrentCoachIdAsync();
            var coach = await _context.Users.OfType<Coach>().FirstOrDefaultAsync(c => c.UserId == coachId);

            var coachFullName = coach?.FullName ?? User.FindFirst("FullName")?.Value ?? "Coach";
            ViewBag.WelcomeMessage = $"Welcome, Coach {coachFullName}!";

            // Get quick stats
            var today = DateTime.Today;
            var thisWeekEnd = today.AddDays(7);

            // This week's class count
            var thisWeekClasses = await _context.Schedules
                .Include(s => s.Class)
                .Where(s => s.CoachId == coachId &&
                           s.StartDate >= today &&
                           s.StartDate <= thisWeekEnd)
                .CountAsync();

            // Total students
            var totalStudents = await _context.Enrollments
                .Include(e => e.Schedule)
                .Where(e => e.Schedule.CoachId == coachId &&
                           e.Status == "Active")
                .Select(e => e.StudentId)
                .Distinct()
                .CountAsync();

            // Today's attendance to mark
            var todaySchedules = await _context.Schedules
                .Where(s => s.CoachId == coachId &&
                           s.StartDate.Date == today.Date)
                .CountAsync();

            ViewBag.ThisWeekClasses = thisWeekClasses;
            ViewBag.TotalStudents = totalStudents;
            ViewBag.TodaysSchedules = todaySchedules;

            return View();
        }

        // 2. THIS WEEK'S CLASSES - Fixed coach ID retrieval
        public async Task<IActionResult> ThisWeekClasses()
        {
            var coachId = await GetCurrentCoachIdAsync();

            Console.WriteLine($"=== ThisWeekClasses ===");
            Console.WriteLine($"Coach ID: {coachId}");

            var today = DateTime.Today;
            var endOfWeek = today.AddDays(7);

            // Debug: Check if coach exists
            var coach = await _context.Users.OfType<Coach>().FirstOrDefaultAsync(c => c.UserId == coachId);
            Console.WriteLine($"Coach found: {coach != null}, Name: {coach?.FullName}");

            var schedules = await _context.Schedules
                .Include(s => s.Class)
                .Where(s => s.CoachId == coachId &&
                           s.StartDate >= today &&
                           s.StartDate <= endOfWeek)
                .OrderBy(s => s.StartDate)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            Console.WriteLine($"Schedules found: {schedules.Count}");

            // Debug each schedule
            foreach (var schedule in schedules)
            {
                Console.WriteLine($"Schedule ID: {schedule.ScheduleId}, Class: {schedule.Class?.ClassName}");
            }

            var scheduleVMs = schedules.Select(s => new CoachScheduleVM
            {
                ScheduleId = s.ScheduleId,
                ClassName = s.Class?.ClassName ?? "Unknown",
                Level = s.Class?.Level ?? "Unknown",
                ScheduleDate = s.StartDate,
                DayOfWeek = s.StartDate.DayOfWeek.ToString(),
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                PoolLocation = s.PoolLocation,
                CurrentEnrollment = s.CurrentEnrollment,
                MaxCapacity = s.Class?.MaxCapacity ?? 0
            }).ToList();

            ViewBag.WeekStart = today.ToString("MMM dd");
            ViewBag.WeekEnd = endOfWeek.ToString("MMM dd");
            ViewBag.CoachId = coachId;
            ViewBag.TotalSchedules = schedules.Count;

            return View(scheduleVMs);
        }

        // 3. TAKE ATTENDANCE (GET - Show form)
        public async Task<IActionResult> TakeAttendance(int id)
        {
            var coachId = await GetCurrentCoachIdAsync();

            var schedule = await _context.Schedules
                .Include(s => s.Class)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(s => s.ScheduleId == id && s.CoachId == coachId);

            if (schedule == null)
            {
                TempData["ErrorMessage"] = "Class not found or you don't have access!";
                return RedirectToAction("ThisWeekClasses");
            }

            var attendanceList = new List<AttendanceVM>();

            foreach (var enrollment in schedule.Enrollments)
            {
                Console.WriteLine($"Enrollment Status = {enrollment.Status}");

                attendanceList.Add(new AttendanceVM
                {
                    EnrollmentId = enrollment.EnrollmentId,
                    StudentId = enrollment.StudentId,
                    StudentName = enrollment.Student?.FullName ?? "Unknown",
                    CurrentLevel = (enrollment.Student as Student)?.Level ?? "Beginner",
                    IsPresent = enrollment.AttendanceStatus == "Present",
                    AttendanceStatus = enrollment.AttendanceStatus
                });
            }


            ViewBag.ScheduleId = id;
            ViewBag.ClassName = schedule.Class?.ClassName;
            ViewBag.ClassDate = schedule.StartDate.ToString("dddd, MMMM dd");
            ViewBag.ClassTime = $"{schedule.StartTime:hh\\:mm} - {schedule.EndTime:hh\\:mm}";


            return View(attendanceList);
        }

        // 4. SAVE ATTENDANCE (POST - Process form)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAttendance(int id, List<AttendanceVM> attendanceList)
        {
            var coachId = await GetCurrentCoachIdAsync();

            var schedule = await _context.Schedules
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.ScheduleId == id && s.CoachId == coachId);

            if (schedule == null)
            {
                TempData["ErrorMessage"] = "Class not found or you don't have access!";
                return RedirectToAction("ThisWeekClasses");
            }

            // Update database
            foreach (var attendance in attendanceList)
            {
                var enrollment = await _context.Enrollments
                    .FindAsync(attendance.EnrollmentId);

                if (enrollment != null)
                {
                    enrollment.AttendanceStatus = attendance.IsPresent ? "Present" : "Absent";
                    enrollment.UpdatedAt = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();

            // Save to text file
            SaveToTextFile(id, schedule, attendanceList);

            TempData["SuccessMessage"] = $"Attendance saved for {schedule.Class?.ClassName}!";
            return RedirectToAction("ThisWeekClasses");
        }

        // 5. VIEW ATTENDANCE FILES
        public IActionResult ViewAttendanceFiles()
        {
            var files = Directory.GetFiles(_attendanceFolder, "*.txt")
                .Select(Path.GetFileName)
                .OrderByDescending(f => f)
                .ToList();

            return View(files);
        }

        // 6. VIEW SINGLE ATTENDANCE FILE
        public IActionResult ViewAttendanceFile(string filename)
        {
            var filePath = Path.Combine(_attendanceFolder, filename);

            if (!System.IO.File.Exists(filePath))
            {
                TempData["ErrorMessage"] = "File not found!";
                return RedirectToAction("ViewAttendanceFiles");
            }

            var content = System.IO.File.ReadAllText(filePath);
            ViewBag.Filename = filename;

            return View((object)content);
        }

        // 7. MY STUDENTS
        public async Task<IActionResult> MyStudents()
        {
            var coachId = await GetCurrentCoachIdAsync();

            var studentIds = await _context.Enrollments
                .Include(e => e.Schedule)
                .Where(e => e.Schedule.CoachId == coachId)
                .Select(e => e.StudentId)
                .Distinct()
                .ToListAsync();

            var students = new List<StudentProgressVM>();

            foreach (var studentId in studentIds)
            {
                var student = await _context.Users
                    .OfType<Student>()
                    .FirstOrDefaultAsync(s => s.UserId == studentId);

                if (student != null)
                {
                    var enrollments = await _context.Enrollments
                        .Include(e => e.Schedule)
                        .Where(e => e.StudentId == studentId &&
                                   e.Schedule.CoachId == coachId)
                        .ToListAsync();

                    var totalClasses = enrollments.Count;
                    var attendedClasses = enrollments.Count(e => e.AttendanceStatus == "Present");

                    students.Add(new StudentProgressVM
                    {
                        StudentId = student.UserId,
                        FullName = student.FullName,
                        Email = student.Email,
                        Phone = student.Phone,
                        CurrentLevel = student.Level,
                        AttendanceCount = attendedClasses,
                        TotalClasses = totalClasses
                    });
                }
            }

            return View(students.OrderBy(s => s.FullName).ToList());
        }

        // ===== PRIVATE HELPER METHODS =====

        private async Task<string> GetCurrentCoachIdAsync()
        {
            // Method 1: Try to get CoachId from claims
            var coachIdClaim = User.FindFirst("CoachId") ?? User.FindFirst(ClaimTypes.NameIdentifier);

            if (coachIdClaim != null && !string.IsNullOrEmpty(coachIdClaim.Value))
            {
                // Check if this is a valid coach ID
                var coachExists = await _context.Users.OfType<Coach>()
                    .AnyAsync(c => c.UserId == coachIdClaim.Value);

                if (coachExists)
                {
                    Console.WriteLine($"Found Coach ID from claims: {coachIdClaim.Value}");
                    return coachIdClaim.Value;
                }
            }

            // Method 2: Get by email (since you store email in claims)
            var email = User.Identity?.Name;
            if (!string.IsNullOrEmpty(email))
            {
                var coach = await _context.Users.OfType<Coach>()
                    .FirstOrDefaultAsync(c => c.Email == email);

                if (coach != null)
                {
                    Console.WriteLine($"Found Coach by email {email}: ID = {coach.UserId}");
                    return coach.UserId;
                }
            }

            // Method 3: Try to find any coach with current user's name
            var fullName = User.FindFirst("FullName")?.Value;
            if (!string.IsNullOrEmpty(fullName))
            {
                var coach = await _context.Users.OfType<Coach>()
                    .FirstOrDefaultAsync(c => c.FullName == fullName);

                if (coach != null)
                {
                    Console.WriteLine($"Found Coach by name {fullName}: ID = {coach.UserId}");
                    return coach.UserId;
                }
            }

            Console.WriteLine($"ERROR: Could not find Coach ID for user {User.Identity?.Name}");
            return string.Empty;
        }

        private void SaveToTextFile(int scheduleId, Schedule schedule, List<AttendanceVM> attendanceList)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"attendance_{timestamp}_{scheduleId}.txt";
            var filePath = Path.Combine(_attendanceFolder, filename);

            var content = new StringBuilder();
            content.AppendLine("=== SWIMMING CLASS ATTENDANCE ===");
            content.AppendLine($"Class: {schedule.Class?.ClassName}");
            content.AppendLine($"Date: {schedule.StartDate:yyyy-MM-dd}");
            content.AppendLine($"Time: {schedule.StartTime:hh\\:mm} - {schedule.EndTime:hh\\:mm}");
            content.AppendLine($"Coach: {User.FindFirst("FullName")?.Value ?? User.Identity?.Name}");
            content.AppendLine($"Location: {schedule.PoolLocation}");
            content.AppendLine("----------------------------------");
            content.AppendLine("ATTENDANCE LIST:");

            int counter = 1;
            foreach (var student in attendanceList)
            {
                var status = student.IsPresent ? "✓ PRESENT" : "✗ ABSENT";
                content.AppendLine($"{counter}. {student.StudentName} - {status}");
                counter++;
            }

            content.AppendLine("----------------------------------");
            content.AppendLine($"Total Present: {attendanceList.Count(s => s.IsPresent)}");
            content.AppendLine($"Total Absent: {attendanceList.Count(s => !s.IsPresent)}");
            content.AppendLine($"Recorded at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            System.IO.File.WriteAllText(filePath, content.ToString());
        }
    }
}
