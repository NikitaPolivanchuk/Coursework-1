using E_Shop.Data.Services;
using E_Shop.Models;
using E_Shop.Services;
using System.Net;
using System.Text;
using Webserver.Controllers;
using Webserver.Controllers.Content;
using Webserver.Sessions;

namespace E_Shop.Controllers
{
    internal class CartController : Controller
    {
        private readonly string _body;
        private readonly string _cartItem;

        private readonly IUserService _userService;
        private readonly ICartService _cartService;
        private readonly IProductService _productService;
        private readonly LayoutBuilder _layoutBuilder;

        public CartController(
            IUserService userService,
            ICartService cartService,
            IProductService productService,
            LayoutBuilder layoutBuilder)
        {
            _userService = userService;
            _cartService = cartService;
            _productService = productService;
            _layoutBuilder = layoutBuilder;

            _body = File.ReadAllText($"{AbsolutePath}Views/Cart/_body.html");
            _cartItem = File.ReadAllText($"{AbsolutePath}Views/Cart/_cartItem.html");
        }

        [Endpoint("GET", "Cart/Index")]
        public IActionResult Index(Session session)
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

            Dictionary<Product, int> cartItems = _cartService.GetCartItems(user.Id);
            StringBuilder sb = new StringBuilder();

            double totalPrice = 0;
            string disabled = "";

            if (cartItems.Count < 1)
            {
                sb.AppendLine("<p>Cart is empty</p>");
                disabled = "disabled";
            }
            else
            {
                string navbtns = File.ReadAllText($"{AbsolutePath}Views/Cart/_navbtns.html");
                sb.AppendLine(navbtns);
            }

            foreach (var item in cartItems)
            {
                Product product = item.Key;
                totalPrice += product.Price * item.Value;

                string price = string.Format("{0:F2}$", product.Price);

                sb.AppendLine(string.Format(_cartItem, product.ImageUrl, product.Name, item.Value, price, product.Id));
            }

            string totalPriceF = string.Format("{0:F2}$", totalPrice);

            string body = string.Format(_body, sb.ToString(), totalPriceF, disabled);

            return View("Cart", "Cart/index.html", user.Username, body);
        }

        [Endpoint("GET", "Cart/Add/{productId:int}")]
        public IActionResult Add(Session session, int productId)
        {
            if (!session.Authorized)
            {
                return Error(HttpStatusCode.Unauthorized);
            }

            User? user = _userService.Get(int.Parse(session.Properties["id"]));
            Product? product = _productService.Get(productId);

            if (user == null || product == null)
            {
                return Error(HttpStatusCode.NotFound);
            }

            _cartService.Add(user.Id, product.Id);

            _layoutBuilder.Configure(session);
            return Redirect("../Index");
        }

        [Endpoint("GET", "Cart/Remove/{productId:int}")]
        public IActionResult Remove(Session session, int productId)
        {
            if (!session.Authorized)
            {
                return Error(HttpStatusCode.Unauthorized);
            }

            User? user = _userService.Get(int.Parse(session.Properties["id"]));
            Product? product = _productService.Get(productId);

            if (user == null || product == null)
            {
                return Redirect("../../Home/Index");
            }

            _cartService.Delete(user.Id, product.Id);

            _layoutBuilder.Configure(session);
            return Redirect("../Index");
        }

        [Endpoint("GET", "Cart/RemoveAll")]
        public IActionResult RemoveAll(Session session)
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

            _cartService.DeleteAll(user.Id);

            _layoutBuilder.Configure(session);
            return Redirect("Index");
        }

        [Endpoint("GET", "Cart/Continue")]
        public IActionResult Continue(Session session)
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

            Dictionary<Product, int> cartItems = _cartService.GetCartItems(user.Id);

            foreach (Product product in cartItems.Keys)
            {
                if (product.Number < cartItems[product])
                {
                    return Redirect("Index");
                }
            }
            return Redirect("../Order/Cart");
        }
    }
}
