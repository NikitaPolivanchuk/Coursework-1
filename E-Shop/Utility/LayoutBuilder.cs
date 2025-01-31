using E_Shop.Controllers;
using Webserver.Content;
using Webserver.Utility;

namespace E_Shop.Utility
{
    public class LayoutBuilder
    {
        public static Dictionary<string, string> Args = new Dictionary<string, string>()
        {
            {"adminTools", "" },
            {"cart", "" },
            {"user", "" }
        };

        public static void Configure(Session session, string absolutePath)
        {
            Args["user"] = session.Authorized
                ? File.ReadAllText($"{absolutePath}Views/_navbarAuth.html")
                : File.ReadAllText($"{absolutePath}Views/_navbarUnAuth.html");

            Args["adminTools"] = string.Empty;

            if (session.Authorized)
            {
                Args["user"] = string.Format(Args["user"], session.Properties["username"]);

                if (session.Properties["access_level"] == "admin")
                {
                    Args["adminTools"] = File.ReadAllText($"{absolutePath}Views/_navbarAdmin.html");
                }
            }

            Args["cart"] = session.Authorized
                ? CartController.UpdateCartNavbar(int.Parse(session.Properties["id"]))
                : string.Empty;

            ViewResult.LayoutArgs = Args.Values.ToArray();
        }
    }
}
