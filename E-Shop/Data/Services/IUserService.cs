using E_Shop.Models;

namespace E_Shop.Data.Services
{
    internal interface IUserService
    {
        public void Add(User user);
        public User? Get(int id);
        public User? Get(string email);
        public User? Get(string email, string password);
        public User[] GetAll(string accessLevel);
        public void Update(int id, User user);
    }
}
