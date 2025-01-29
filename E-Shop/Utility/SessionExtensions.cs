using E_Shop.Models;
using Webserver.Utility;

namespace E_Shop.Utility
{
    internal static class SessionExtensions
    {
        public static void AddUser(this Session session, User user)
        {
            session.Properties["id"] = user.Id.ToString();
            session.Properties["username"] = user.Username;
            session.Properties["access_level"] = user.AccessLevel;
            session.Authorized = true;
        }

        public static bool IsAdmin(this Session session)
        {
            if (session.Properties.TryGetValue("access_level", out string? value))
            {
                if (value == "admin")
                {
                    return true;
                }
            }
            return false;
        }
    }
}
