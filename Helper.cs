using AQUA.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AQUA.Controllers
{
    public class Helper
    {
        private readonly ApplicationDbContext _db;

        public Helper(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task SignInAsync(HttpContext httpContext, string email, string role,
            bool isPersistent, string? imageUrl = null, string? fullName = null, string? userId = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.NameIdentifier, userId ?? ""),
                new Claim(ClaimTypes.Role, role),
                new Claim("FullName", fullName ?? ""),
                new Claim("ProfilePicture", imageUrl ?? "default.png")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = isPersistent });
        }

        public string GenerateRandomPassword(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public string ValidatePhoto(IFormFile? photo)
        {
            if (photo == null) return "";
            if (photo.Length > 2 * 1024 * 1024) return "Photo must be less than 2MB.";
            if (!photo.ContentType.StartsWith("image/")) return "File must be an image.";
            return "";
        }

        public string SavePhoto(IFormFile photo, string folder = "photos")
        {
            var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder, uniqueName);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var stream = new FileStream(path, FileMode.Create);
            photo.CopyTo(stream);
            return "/" + folder + "/" + uniqueName;
        }

        public void DeletePhoto(string? filename, string folder = "photos")
        {
            if (string.IsNullOrEmpty(filename)) return;
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder, filename);
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }

        public string GenerateUserId(string prefix)
        {
            int count = _db.Users.Count(u => u.UserId.StartsWith(prefix)) + 1;
            return $"{prefix}{count:0000}";
        }
    }
}