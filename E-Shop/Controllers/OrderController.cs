using E_Shop.Data;
using E_Shop.Data.Enums;
using E_Shop.Data.Services;
using E_Shop.Models;
using System.Net;
using System.Net.Mail;
using System.Text;
using Webserver;
using Webserver.Content;
using Webserver.Utility;

namespace E_Shop.Controllers
{
    internal class OrderController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderItemsService _orderItemsService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IUserService _userService;
        private readonly IProductKeyService _productKeyService;

        private readonly string _email_p1;
        private readonly string _email_p2;
        private readonly string _tableRow;

        private bool _clearCart;

        public OrderController(ICartService cartService,
                               IOrderItemsService orderItemsService,
                               IOrderService orderService,
                               IProductService productService,
                               IUserService userService,
                               IProductKeyService productKeyService)
        {
            _cartService = cartService;
            _orderItemsService = orderItemsService;
            _orderService = orderService;
            _productService = productService;
            _userService = userService;
            _productKeyService = productKeyService;

            _email_p1 = File.ReadAllText($"{AbsolutePath}Views/Order/email_p1.html");
            _email_p2 = File.ReadAllText($"{AbsolutePath}Views/Order/email_p2.html");
            _tableRow = File.ReadAllText($"{AbsolutePath}Views/Order/_row.html");
        }

        [Endpoint("GET", "Order/Cart")]
        public IActionResult Cart(Session session)
        {
            if (!session.Authorized)
            {
                return Error(HttpStatusCode.Unauthorized);
            }

            User? user = _userService.Get(int.Parse(session.Properties["id"]));
            if (user == null)
            {
                return Error(HttpStatusCode.NotFound);
            }

            _clearCart = true;

            Dictionary<Product, int> items = _cartService.GetCartItems(user.Id);
            return _InitOrder(user, items);
        }

        [Endpoint("GET", "Order/Direct/{id:int}")]
        public IActionResult Direct(Session session, int id)
        {
            if (!session.Authorized)
            {
                return Error(HttpStatusCode.Unauthorized);
            }

            User? user = _userService.Get(int.Parse(session.Properties["id"]));
            if (user == null)
            {
                return Error(HttpStatusCode.NotFound);
            }

            Product? product = _productService.Get(id);
            if (product == null)
            {
                return Error(HttpStatusCode.NotFound);
            }

            _clearCart = false;

            Dictionary<Product, int> item = new Dictionary<Product, int> { {product, 1} };
            return _InitOrder(user, item);
        }

        private IActionResult _InitOrder(User user, Dictionary<Product, int> items)
        {
            Order? prev = _orderService.GetCurrent(user.Id);

            if (prev != null)
            {
                prev.Status = "Canceled";
                _orderService.Update(prev.Id, prev);
            }

            Order order = new Order(user.Id);

            foreach (Product product in items.Keys)
            {
                order.TotalCost += product.Price * items[product];
            }

            _orderService.Add(order);
            order = _orderService.GetCurrent(user.Id)!;

            foreach (Product product in items.Keys)
            {
                _orderItemsService.Add(new OrderItem(order.Id, product.Id, items[product]));
            }

            return Redirect("../../Payment/Proceed");
        }


        [Endpoint("GET", "Order/Proceed/{code:int}")]
        public IActionResult Completed(Session session, int code)
        {
            if (!session.Authorized)
            {
                return Error(HttpStatusCode.Unauthorized);
            }

            int userId = int.Parse(session.Properties["id"]);

            Order? order = _orderService.GetCurrent(userId);

            if (order == null)
            {
                return Error(HttpStatusCode.NotFound);
            }

            string redirect;
            switch ((OrderStatus)code)
            {
                case OrderStatus.Completed:
                    if (_clearCart)
                    {
                        _cartService.DeleteAll(userId);
                    }
                    _SendOrder(order);

                    order.Status = "Completed";
                    redirect = "../../Home/Index";
                    break;

                case OrderStatus.Canceled:
                    order.Status = "Canceled";
                    redirect = "../../Cart/Index";
                    break;

                default:
                    return Error(HttpStatusCode.BadRequest);
            }

            _orderService.Update(order.Id, order);
            return Redirect(redirect);
        }

        private void _SendOrder(Order order)
        {
            User user = _userService.Get(order.UserId)!;

            string from = GetConfig("ShopEmail")!;
            string to = user.Email;

            MailMessage mail = new MailMessage(from, to);

            mail.Subject = "Thank you for purchase";

            StringBuilder tableData = new StringBuilder();
            StringBuilder keys = new StringBuilder();

            OrderItem[] orderItems = _orderItemsService.GetAll(order.Id);

            foreach (OrderItem item in orderItems)
            {
                Product product = _productService.Get(item.ProductId)!;

                string price = string.Format("{0:F2}$", product.Price);

                tableData.AppendLine(string.Format(_tableRow, product.ImageUrl, product.Name, item.Number, price));

                List<ProductKey> productKeys = _productKeyService.GetAll(product.Id).ToList();
                keys.AppendLine(product.Name);

                for (int i = 0; i < item.Number; i++)
                {
                    ProductKey productKey = productKeys.Last();

                    keys.AppendLine(productKey.Value);

                    _productKeyService.Delete(productKey);
                    productKeys.Remove(productKey);
                }
                keys.AppendLine();

                product.Number -= item.Number;
                _productService.Update(product.Id, product);
            }

            string outputFilePath = $"{AbsolutePath}root/tmp/{order.Id}.txt";

            File.WriteAllText(outputFilePath, keys.ToString());
            mail.Attachments.Add(new Attachment(outputFilePath));

            string totalCost = string.Format("{0:F2}$", order.TotalCost);

            string email_p2 = string.Format(_email_p2, user.Username, order.Id, DateTime.Now, tableData.ToString(), totalCost);

            StringBuilder email = new StringBuilder();
            email.AppendLine(_email_p1);
            email.AppendLine(email_p2);

            mail.Body = email.ToString();
            mail.IsBodyHtml = true;

            EmailSender emailSender = new EmailSender();
            emailSender.Send(mail);

            mail.Attachments.ToList().ForEach(x => x.ContentStream.Dispose());

            File.Delete(outputFilePath);
        }
    }
}
