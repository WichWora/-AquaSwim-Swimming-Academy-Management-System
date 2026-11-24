using AQUA;
using AQUA.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AQUA.Controllers;
using System.Security.Claims;

namespace AQUA.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly Helper _hp;

        public AccountController(ApplicationDbContext db, Helper hp)
        {
            _db = db;
            _hp = hp;
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM vm, string? returnUrl)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == vm.Email);

            if (user == null || user.Password != vm.Password)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(vm);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.UserId),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("FullName", user.FullName),
                new Claim("ProfilePicture", user.ImageUrl ?? "default.png")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = vm.RememberMe
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            TempData["Info"] = "Login successful.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            // If Manager is logged in, show manager registration form
            if (User.IsInRole("Manager"))
            {
                return View("RegisterManager"); // Create a separate view for managers
            }

            // Public registration - only for students
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            // Check if email already exists
            if (await _db.Users.AnyAsync(u => u.Email == vm.Email))
                ModelState.AddModelError("Email", "This email is already registered.");

            // If NOT authenticated (public registration) - only allow Student role
            if (!User.Identity.IsAuthenticated && vm.Role != "Student")
            {
                ModelState.AddModelError("Role", "Only Student accounts can be registered publicly.");
                return View(vm);
            }

            // If authenticated but not Manager - only allow Student role
            if (User.Identity.IsAuthenticated && !User.IsInRole("Manager") && vm.Role != "Student")
            {
                ModelState.AddModelError("Role", "Only Managers can create Coach, ServerAdmin, or Manager accounts.");
                return View(vm);
            }

            // If Manager is creating accounts, validate required fields based on role
            if (User.IsInRole("Manager"))
            {
                // Clear validation errors for Specialization and AccessLevel
                // since Manager can set these
                ModelState.Remove("Specialization");
                ModelState.Remove("AccessLevel");

                // You might want to add custom validation here if needed
                if (vm.Role == "Coach" && string.IsNullOrEmpty(vm.Specialization))
                {
                    ModelState.AddModelError("Specialization", "Specialization is required for Coach.");
                }

                if (vm.Role == "ServerAdmin" && (!vm.AccessLevel.HasValue || vm.AccessLevel <= 0))
                {
                    ModelState.AddModelError("AccessLevel", "Valid Access Level is required for ServerAdmin.");
                }
            }

            if (!ModelState.IsValid)
                return View(vm);

            // Rest of your registration logic remains the same...
            string prefix = vm.Role switch
            {
                "Student" => "S",
                "Coach" => "C",
                "ServerAdmin" => "SA",
                "Manager" => "M",
                _ => "X"
            };

            var newId = _hp.GenerateUserId(prefix);

            UserBase newUser = vm.Role switch
            {
                "Student" => new Student { UserId = newId },
                "Coach" => new Coach
                {
                    UserId = newId,
                    Specialization = vm.Specialization ?? "General",
                    HireDate = DateTime.Now
                },
                "ServerAdmin" => new ServerAdmin
                {
                    UserId = newId,
                    AccessLevel = vm.AccessLevel ?? 100,
                    CreatedDate = DateTime.Now
                },
                "Manager" => new Manager { UserId = newId, AccessLevel = 999 },
                _ => throw new InvalidOperationException("Invalid role")
            };

            newUser.FullName = vm.Name;
            newUser.Email = vm.Email;
            newUser.Phone = vm.Phone;
            newUser.Password = vm.Password;
            newUser.Role = vm.Role;
            newUser.ImageUrl = vm.Photo != null ? _hp.SavePhoto(vm.Photo, "photos") : "default.png";
            newUser.RegistrationDate = DateTime.Now;
            newUser.IsActive = true;

            _db.Users.Add(newUser);
            await _db.SaveChangesAsync();

            // If public registration → auto sign-in
            if (!User.Identity.IsAuthenticated)
            {
                await _hp.SignInAsync(HttpContext, newUser.Email, newUser.Role, true,
                    newUser.ImageUrl, newUser.FullName, newUser.UserId);
                TempData["Info"] = "Registration successful! Welcome!";
            }
            else
            {
                TempData["Info"] = $"{vm.Role} account created successfully.";
            }

            return RedirectToAction("UserProfile");
        }
        // Combined GET: ResetPassword (handles both authenticated and anonymous)
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (User.Identity?.IsAuthenticated == true)
                return View(new PasswordManagementVM { Email = User.Identity.Name });

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(PasswordManagementVM vm)
        {
            if (string.IsNullOrEmpty(vm.NewPassword))
            {
                // Phase 1: Verify email
                if (await _db.Users.AnyAsync(u => u.Email == vm.Email))
                {
                    ViewBag.EmailVerified = true;
                    return View(vm);
                }

                ModelState.AddModelError("Email", "Email not found.");
                return View(vm);
            }

            // Phase 2: Change password
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == vm.Email);
            if (user == null || vm.NewPassword != vm.ConfirmPassword)
            {
                ModelState.AddModelError("", user == null ? "User not found." : "Passwords do not match.");
                ViewBag.EmailVerified = true;
                return View(vm);
            }

            user.Password = vm.NewPassword;
            await _db.SaveChangesAsync();

            TempData["Info"] = "Password updated successfully!";

            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("UserProfile");

            return RedirectToAction("Login");
        }

        // Unified GetCurrentUserAsync – always use TPH table
        private async Task<UserBase?> GetCurrentUserAsync(string email)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        // Update CheckEmail to use Users table
        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> CheckEmail(string email)
            => Json(!await _db.Users.AnyAsync(u => u.Email == email));

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> UpdateProfile()
        {
            var email = User.Identity!.Name!;
            var user = await GetCurrentUserAsync(email);

            if (user == null) return RedirectToAction("Index", "Home");

            if (user is Coach c)
                ViewBag.Spec = c.Specialization;

            return View(new UpdateProfileVM
            {
                Email = user.Email,
                Name = user.FullName,
                Phone = user.Phone,
                PhotoURL = user.ImageUrl
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateProfileVM vm)
        {
            var email = User.Identity!.Name!;
            var user = await GetCurrentUserAsync(email);

            if (user == null) return RedirectToAction("Index", "Home");

            if (vm.Photo != null)
            {
                if (!string.IsNullOrEmpty(user.ImageUrl) && user.ImageUrl != "/photos/default.png")
                    _hp.DeletePhoto(user.ImageUrl.TrimStart('/'), "photos"); // remove leading /

                user.ImageUrl = _hp.SavePhoto(vm.Photo, "photos"); // will return /photos/filename
            }

            await _db.SaveChangesAsync();

            if (!ModelState.IsValid)
            {
                // Reload the user from DB to get current values
                var currentUser = await GetCurrentUserAsync(User.Identity!.Name!);

                var vmReturn = new UpdateProfileVM
                {
                    Email = currentUser.Email,
                    Name = currentUser.FullName,
                    Phone = currentUser.Phone,
                    PhotoURL = currentUser.ImageUrl
                };

                // Include Specialization if Coach
                if (currentUser is Coach coachUser)
                    vmReturn.Specialization = coachUser.Specialization;

                return View(vmReturn);
            }

            user.FullName = vm.Name;
            user.Phone = vm.Phone;

            if (vm.Photo != null)
            {
                if (!string.IsNullOrEmpty(user.ImageUrl) && user.ImageUrl != "default.png")
                    _hp.DeletePhoto(user.ImageUrl, "photos");

                user.ImageUrl = _hp.SavePhoto(vm.Photo, "photos");
            }

            if (user is Coach c)
            {
                c.Specialization = vm.Specialization; // Add Specialization to UpdateProfileVM
            }

            await _db.SaveChangesAsync();
            await _hp.SignInAsync(HttpContext, user.Email, user.Role, true, user.ImageUrl, user.FullName, user.UserId);

            TempData["Info"] = "Profile updated successfully.";
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> UserProfile()
        {
            var email = User.Identity!.Name!;
            var user = await GetCurrentUserAsync(email);

            if (user == null) return RedirectToAction("Index", "Home");

            if (user is Student s) await _db.Entry(s).Collection(x => x.Enrollments).LoadAsync();
            if (user is Coach c) await _db.Entry(c).Collection(x => x.Schedules).LoadAsync();

            return View(user);
        }
    }
}