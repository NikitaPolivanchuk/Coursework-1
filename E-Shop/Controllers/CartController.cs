using E_Shop.Data.Services;
using E_Shop.Models;
using E_Shop.Utility;
using System.Net;
using System.Text;
using Webserver;
using Webserver.Content;
using Webserver.Utility;

namespace E_Shop.Controllers
{
    internal class CartController : Controller
    {
        private static readonly string _cart =
            File.ReadAllText($"{AbsolutePath}Views/Cart/_cart.html");
        private static readonly string _dropdown =
            File.ReadAllText($"{AbsolutePath}Views/Cart/_dropdown.html");
        private static readonly string _total =
            File.ReadAllText($"{AbsolutePath}Views/Cart/_total.html");

        private static ICartService _cartServiceSt = new CartService(new ProductService());

        private readonly string _body;
        private readonly string _cartItem;

        private readonly IUserService _userService;
        private readonly ICartService _cartService;
        private readonly IProductService _productService;

        public CartController(IUserService userService,
                              ICartService cartService,
                              IProductService productService)
        {
            _userService = userService;
            _cartService = cartService;
            _productService = productService;

            _body = File.ReadAllText($"{AbsolutePath}Views/Cart/_body.html");
            _cartItem = File.ReadAllText($"{AbsolutePath}Views/Cart/_cartItem.html");
        }

        public static string UpdateCartNavbar(int userId)
        {
            int count = 0;
            double price = 0;
            StringBuilder dropdownItems = new StringBuilder();

            Dictionary<Product, int> cartItems = _cartServiceSt.GetCartItems(userId);

            foreach (var item in cartItems)
            {
                Product product = item.Key;
                string body = $"{product.Name} - {item.Value}";

                count++;
                price += product.Price * item.Value;
                dropdownItems.AppendLine(string.Format(_dropdown, product.Id, body));
            }

            if (count > 0)
            {
                string total = string.Format("Total: ${0:F2}", price);
                dropdownItems.AppendLine(string.Format(_total, total));
            }
            else
            {
                string empty = File.ReadAllText($"{AbsolutePath}Views/Cart/_emptyNavbar.html");
                dropdownItems.AppendLine(empty);
            }

            return string.Format(_cart, count, dropdownItems.ToString());
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

            LayoutBuilder.Configure(session, AbsolutePath);
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

            LayoutBuilder.Configure(session, AbsolutePath);
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

            LayoutBuilder.Configure(session, AbsolutePath);
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
