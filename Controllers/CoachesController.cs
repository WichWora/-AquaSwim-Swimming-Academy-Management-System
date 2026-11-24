using AQUA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AQUA.Controllers
{
    public class CoachesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoachesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Coaches
        public async Task<IActionResult> Index()
        {
            var coaches = await _context.Users
                .OfType<Coach>()
                .Where(c => c.IsActive)
                .ToListAsync();

            return View(coaches);
        }

        // GET: Coaches/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Coaches/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("FullName,Specialization,Email,Phone")] Coach coach)
        {
            if (!ModelState.IsValid)
                return View(coach);

            coach.UserId = $"C{DateTime.Now.Ticks % 100000:D5}";
            coach.Role = "Coach";
            coach.HireDate = DateTime.Now;
            coach.IsActive = true;
            coach.RegistrationDate = DateTime.Now;

            _context.Users.Add(coach);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
