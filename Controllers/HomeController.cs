using Microsoft.AspNetCore.Mvc;
using System;

namespace AQUA.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            // Check if user is on mobile device
            var isMobile = IsMobileDevice(Request);
            ViewBag.IsMobile = isMobile;

            return View();
        }

        // Method to detect mobile devices
        private bool IsMobileDevice(HttpRequest request)
        {
            var userAgent = request.Headers["User-Agent"].ToString();

            // Common mobile keywords
            var mobileKeywords = new[]
            {
                "mobile", "android", "iphone", "phone", "ipod",
                "blackberry", "windows phone", "iemobile", "opera mini"
            };

            var lowerAgent = userAgent.ToLower();
            return mobileKeywords.Any(keyword => lowerAgent.Contains(keyword));
        }
    }
}