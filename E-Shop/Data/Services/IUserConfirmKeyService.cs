using E_Shop.Models;

namespace E_Shop.Data.Services
{
    internal interface IUserConfirmKeyService
    {
        public void Add(int userId, string key);
        public User? Get(string key);
        public User? Get(int userId);
        public void Update(int userId, string key);
        public void Delete(int userId);
    }
}
