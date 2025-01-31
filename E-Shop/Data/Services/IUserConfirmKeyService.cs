using E_Shop.Models;

namespace E_Shop.Data.Services
{
    internal interface IUserConfirmKeyService
    {
        public void Add(UserConfirmKey confirmKey);
        public User? Get(string key);
        public User? Get(int userId);
        public void Update(UserConfirmKey confirmKey);
        public void Delete(int userId);
    }
}
