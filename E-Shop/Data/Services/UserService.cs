using E_Shop.Models;
using E_Shop.Services;
using System.Data;

namespace E_Shop.Data.Services
{
    internal class UserService : IUserService
    {
        private readonly DbAccess _db;

        public UserService()
        {
            _db = DbAccess.GetInstance();
        }

        public void Add(User user)
        {
            _db.Insert("Users", ["username", "email", "password", "access_level", "verified"],
                [user.Username, user.Email, user.Password, user.AccessLevel, user.Verified.ToString()]);
        }

        public User? Get(int id)
        {
            return _Get($"id={id}");
        }

        public User? Get(string email)
        {
            return _Get($"email='{email}'");
        }

        public User? Get(string email, string password)
        {
            return _Get($"email='{email}' AND password='{password}'");
        }
        public User[] GetAll(string accessLevel)
        {
            DataTable data = _db.Select("Users", ["*"], $"access_level='{accessLevel}'");
            List<User> users = new List<User>();

            foreach (DataRow row in data.Rows)
            {
                users.Add(new User(row));
            }
            return users.ToArray();
        }

        public void Update(int id, User user)
        {
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                {"username", user.Username},
                {"password", user.Password},
                {"verified", user.Verified.ToString() }
            };

            _db.Update("Users", data, $"id={id}");
        }

        private User? _Get(string condition)
        {
            DataTable table = _db.Select("Users", ["*"], condition);

            if (table.Rows.Count < 1)
            {
                return null;
            }

            return new User(table.Rows[0]);
        }
    }
}
