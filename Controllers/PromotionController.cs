using Microsoft.AspNetCore.Mvc;

namespace AQUA.Controllers
{
    public class PromotionController : Controller
    {
        

        // This will serve: /Promotion/Promotion
        public IActionResult Promotion()
        {
            return View();
        }
    }
}