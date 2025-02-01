using System.Text;
using E_Shop.Data.Services;
using E_Shop.Models;
using Webserver.Controllers.Content;
using Webserver.DependencyInjection;
using Webserver.Sessions;

namespace E_Shop.Services
{
    public class LayoutBuilder
    {
        private readonly string absolutePath;
        private readonly IServiceCollectionProvider serviceProvider;
        private readonly Dictionary<string, string> args = new()
        {
            {"adminTools", "" },
            {"cart", "" },
            {"user", "" }
        };

        private readonly string _cart;
        private readonly string _dropdown;
        private readonly string _total;

        public LayoutBuilder(IServiceCollectionProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;

            absolutePath = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName;

            _cart = File.ReadAllText($"{absolutePath}/Views/Cart/_cart.html");
            _dropdown = File.ReadAllText($"{absolutePath}/Views/Cart/_dropdown.html");
            _total = File.ReadAllText($"{absolutePath}/Views/Cart/_total.html");
        }

        public void Configure(Session session)
        {
            args["user"] = session.Authorized
                ? File.ReadAllText($"{absolutePath}/Views/_navbarAuth.html")
                : File.ReadAllText($"{absolutePath}/Views/_navbarUnAuth.html");

            args["adminTools"] = string.Empty;

            if (session.Authorized)
            {
                args["user"] = string.Format(args["user"], session.Properties["username"]);

                if (session.Properties["access_level"] == "admin")
                {
                    args["adminTools"] = File.ReadAllText($"{absolutePath}/Views/_navbarAdmin.html");
                }
            }

            args["cart"] = session.Authorized
                ? UpdateCartNavbar(int.Parse(session.Properties["id"]))
                : string.Empty;

            ViewResult.LayoutArgs = args.Values.ToArray();
        }

        private string UpdateCartNavbar(int userId)
        {
            int count = 0;
            double price = 0;
            StringBuilder dropdownItems = new StringBuilder();

            Dictionary<Product, int> cartItems = serviceProvider.GetService<ICartService>().GetCartItems(userId);

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
                string empty = File.ReadAllText($"{absolutePath}/Views/Cart/_emptyNavbar.html");
                dropdownItems.AppendLine(empty);
            }

            return string.Format(_cart, count, dropdownItems.ToString());
        }
    }
}
