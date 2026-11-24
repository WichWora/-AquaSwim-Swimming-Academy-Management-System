// Controllers/SubscriptionController.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using AQUA.Models;

namespace AQUA.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly InMemoryOrderConnector _orderConnector;
        private readonly ILogger<SubscriptionController> _logger;

        public SubscriptionController(
            InMemoryOrderConnector orderConnector,
            ILogger<SubscriptionController> logger)
        {
            _orderConnector = orderConnector;
            _logger = logger;
        }

        // GET: /subscribe
        [HttpGet]
        [Route("subscribe")]
        public IActionResult Index()
        {
            ViewBag.QueueCount = _orderConnector.GetQueueCount();
            return View();
        }

        // POST: /subscribe
        [HttpPost]
        [Route("subscribe")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscribe(string email, string name)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.ErrorMessage = "Please enter a valid email address.";
                ViewBag.QueueCount = _orderConnector.GetQueueCount();
                return View("Index");
            }

            if (!email.Contains("@") || !email.Contains("."))
            {
                ViewBag.ErrorMessage = "Please enter a valid email address.";
                ViewBag.QueueCount = _orderConnector.GetQueueCount();
                return View("Index");
            }

            // Create a subscription "order"
            var order = new OrderInfo
            {
                CustomerName = string.IsNullOrWhiteSpace(name) ? "Subscriber" : name,
                CustomerEmail = email,
                QuantityOrdered = 1,
                ItemName = "Newsletter Subscription"
            };

            // Add to queue for processing
            await _orderConnector.AddOrder(order);

            _logger.LogInformation("New subscription request from {Email}", email);

            ViewBag.SuccessMessage = "Thank you for subscribing! Please check your email for confirmation.";
            ViewBag.QueueCount = _orderConnector.GetQueueCount();
            return View("Index");
        }

        // GET: /subscribe/status
        [HttpGet]
        [Route("subscribe/status")]
        public IActionResult Status()
        {
            var count = _orderConnector.GetQueueCount();
            var orders = _orderConnector.GetAllOrders();

            ViewBag.OrderCount = count;
            ViewBag.Orders = orders;

            return View();
        }

        // GET: /subscribe/test
        [HttpGet]
        [Route("subscribe/test")]
        public IActionResult Test()
        {
            return Content("Subscription Controller is working!");
        }
    }
}