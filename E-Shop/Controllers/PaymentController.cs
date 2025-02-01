using E_Shop.Data.Enums;
using E_Shop.Data.Services;
using E_Shop.Models;
using System.Net;
using Webserver.Controllers;
using Webserver.Controllers.Content;
using Webserver.Sessions;

namespace E_Shop.Controllers
{
    internal class PaymentController : Controller
    {
        private readonly string _proceedPage;

        private readonly IOrderService _orderService;

        public PaymentController(IOrderService orderService)
        {
            _proceedPage = _proceedPage = File.ReadAllText($"{AbsolutePath}Views/Payment/index.html");

            _orderService = orderService;
        }

        [Endpoint("GET", "Payment/Proceed")]
        public string? Proceed(Session session)
        {
            if (!session.Authorized)
            {
                return null;
            }

            int userId = int.Parse(session.Properties["id"]);

            Order? order = _orderService.GetCurrent(userId);

            if (order == null)
            {
                return null;
            }

            string price = string.Format("{0:F2}$", order.TotalCost);

            return string.Format(_proceedPage, price);
        }

        [Endpoint("POST", "Payment/Confirmed")]
        public IActionResult Confirmed(Session session)
        {
            if (!session.Authorized)
            {
                return Error(HttpStatusCode.Unauthorized);
            }
            return Redirect($"../Order/Proceed/{(int)OrderStatus.Completed}");
        }

        [Endpoint("GET", "Payment/Canceled")]
        public IActionResult Canceled(Session session)
        {
            if (!session.Authorized)
            {
                return Error(HttpStatusCode.Unauthorized);
            }
            return Redirect($"../Order/Proceed/{(int)OrderStatus.Canceled}");
        }
    }
}
