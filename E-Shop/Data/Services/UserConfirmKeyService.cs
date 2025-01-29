using E_Shop.Models;
using E_Shop.Services;
using System.Data;

namespace E_Shop.Data.Services
{
    internal class UserConfirmKeyService : IUserConfirmKeyService
    {
        private readonly DbAccess _db;
        private readonly IUserService _userService;

        public UserConfirmKeyService(IUserService userService)
        {
            _db = DbAccess.GetInstance();
            _userService = userService;
        }

        public void Add(int userId, string key)
        {
            _db.Insert("UserConfirmKeys", ["user_id", "secret_key"], [userId.ToString(), key]);
        }

        public void Delete(int userId)
        {
            _db.Delete("UserConfirmKeys", $"user_id={userId}");
        }

        public User? Get(string key)
        {
            DataTable data = _db.Select("UserConfirmKeys", ["*"], $"secret_key='{key}'");
            
            if (data.Rows.Count < 1)
            {
                return null;
            }

            User? user = _userService.Get((int)data.Rows[0][0]);
            return (user == null) ? null : user;
        }

        public User? Get(int userId)
        {
            DataTable data = _db.Select("UserConfirmKeys", ["*"], $"user_id='{userId}'");

            if (data.Rows.Count < 1)
            {
                return null;
            }

            User? user = _userService.Get((int)data.Rows[0][0]);
            return (user == null) ? null : user;
        }

        public void Update(int userId, string key)
        {
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "user_id", userId.ToString() },
                { "secret_key", key }
            };

            _db.Update("UserConfirmKeys", data, $"user_id={userId}");
        }
    }
}
